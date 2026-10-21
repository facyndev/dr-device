using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Data;
using Infrastructure.Services;
using Domain.Interfaces;
using Application.Interfaces;
using Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Presentation.Middlewares;
using Presentation.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Contenedor de inyección de dependencias
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISpecializationService, SpecializationService>();
builder.Services.AddScoped<ISpecializationRepository, SpecializationRepository>();
builder.Services.AddScoped<IHasher, BcryptHasher>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<ITransactionManager, TransactionManager>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("password-reset", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            // maximo de peticiones permitidas por ventana de tiempo
            PermitLimit = 5,
            // ventana de tiempo en la que se permite un maximo de peticiones
            Window = TimeSpan.FromMinutes(15),
            // cantidad de peticiones que se pueden agrupar en una cola mientras se espera a que se libere espacio en la ventana de tiempo
            QueueLimit = 0
        }));
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    { 
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
                    context.Token = context.Request.Cookies[CookieConstants.AccessToken];
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters 
        { 
            ValidateIssuer = true, 
            ValidateAudience = true, 
            ValidateIssuerSigningKey = true, 
            ValidateLifetime = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Authentication:Issuer"], 
            ValidAudience = builder.Configuration["Authentication:Audience"], 
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretForKey"]!))
        }; 
    });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
