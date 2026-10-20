using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Application.Models.Auth.Requests;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using Presentation.Controllers;

var connection = Environment.GetEnvironmentVariable("PASSWORD_RESET_TEST_CONNECTION")
    ?? throw new InvalidOperationException("Definir PASSWORD_RESET_TEST_CONNECTION con una base de pruebas vacía.");
if (!(new NpgsqlConnectionStringBuilder(connection).Database?.Contains("test", StringComparison.OrdinalIgnoreCase) ?? false))
    throw new InvalidOperationException("El nombre de la base debe incluir 'test'. No usar una base compartida.");
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options;
ApplicationDbContext Db() => new(options);
await using (var setup = Db())
{
    await setup.Database.MigrateAsync();
    if (await setup.Users.AnyAsync()) throw new InvalidOperationException("La base debe estar vacía.");
}
var clock = new TestClock();
var issuedTokens = new ConcurrentQueue<(string Email, string Token)>();
var hasher = new BcryptHasher();
var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Authentication:Issuer"] = "tests",
    ["Authentication:Audience"] = "tests",
    ["Authentication:SecretForKey"] = "test-only-signing-key-with-at-least-32-bytes"
}).Build();
var jwt = new JwtService(config);
var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed++;
    Console.WriteLine("PASS: " + name);
}
string Token(string email) => issuedTokens.Last(item => item.Email == email).Token;
AuthService Service(ApplicationDbContext db) => new(new UserRepository(db), jwt,
    hasher, new PasswordResetTokenRepository(db), clock, new TransactionManager(db));
async Task<User> CreateUser(bool suspended = false)
{
    var user = new User("Test", "User", Guid.NewGuid() + "@example.com", hasher.Hash("old-password"));
    if (suspended) user.SoftDelete();
    await using var db = Db();
    await new UserRepository(db).Create(user);
    return user;
}
async Task Request(User user)
{
    await using var db = Db();
    var token = await Service(db).ForgotPasswordAsync(new ForgotPasswordRequest { Email = user.Email.ToUpperInvariant() });
    if (token is not null) issuedTokens.Enqueue((user.Email, token));
}
async Task<bool> Reset(string token)
{
    await using var db = Db();
    try
    {
        await Service(db).ResetPasswordAsync(new ResetPasswordRequest
        {
            Token = token, NewPassword = "new-password", ConfirmPassword = "new-password"
        });
        return true;
    }
    catch (ValidationException) { return false; }
}
string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

var active = await CreateUser();
var suspended = await CreateUser(true);
await Request(suspended);
await using (var db = Db())
{
    await Service(db).ForgotPasswordAsync(new ForgotPasswordRequest { Email = "missing@example.com" });
    Check(issuedTokens.IsEmpty && !await db.PasswordResetTokens.AnyAsync(), "Cuentas inexistentes y suspendidas no generan tokens");
}
await Request(active);
var raw = Token(active.Email);
await using (var db = Db())
{
    var stored = await db.PasswordResetTokens.SingleAsync();
    Check(raw.Length == 43 && stored.TokenHash == Hash(raw) && stored.TokenHash != raw, "Solo se guarda el hash SHA-256 del token aleatorio de 256 bits");
    Check(stored.ExpiresAt - stored.CreatedAt == TimeSpan.FromMinutes(10), "El token vence exactamente a los diez minutos");
    Check(await new UserRepository(db).GetByEmail(active.Email.ToUpperInvariant()) is not null, "La solicitud encuentra el correo sin distinguir mayúsculas");
}
await Request(active);
Check(issuedTokens.Count == 1, "Una cuenta no puede solicitar dos tokens dentro de un minuto");
clock.Advance(TimeSpan.FromSeconds(61));
await Request(active);
var replacement = Token(active.Email);
Check(replacement != raw && !await Reset(raw), "Una nueva solicitud invalida el token anterior");
Check(!await Reset(new string('A', 43)), "Un token desconocido no cambia la contraseña");
var oldJwt = jwt.GenerateToken(active, active.Role.ToString());
Check(await Reset(replacement), "Un token válido permite cambiar la contraseña");
await using (var db = Db())
{
    var changed = await db.Users.SingleAsync(u => u.Id == active.Id);
    Check(hasher.Verify("new-password", changed.Password) && !hasher.Verify("old-password", changed.Password), "La nueva contraseña reemplaza la anterior usando BCrypt");
    Check(changed.DeletedAt is null, "El cambio mantiene activa la cuenta");
    Check(!await db.PasswordResetTokens.AnyAsync(t => t.UserId == active.Id && t.UsedAt == null), "Todos los tokens pendientes quedan consumidos");
    var newJwt = new JwtSecurityTokenHandler().ReadJwtToken(jwt.GenerateToken(changed, changed.Role.ToString()));
    Check(!newJwt.Claims.Any(c => c.Type == "session_version"), "Los JWT no requieren una versión de sesión");
    Check(jwt.ValidateToken(oldJwt), "El cambio de contraseña conserva la validez del JWT anterior hasta su vencimiento");
}
Check(!await Reset(replacement), "El mismo enlace no se puede reutilizar");


var expiring = await CreateUser();
await Request(expiring);
clock.Advance(TimeSpan.FromMinutes(10));
Check(!await Reset(Token(expiring.Email)), "El token se rechaza exactamente en su vencimiento");
await using (var db = Db())
{
    Check(hasher.Verify("old-password", (await db.Users.SingleAsync(u => u.Id == expiring.Id)).Password), "Un token vencido no modifica la contraseña");
}
var laterSuspended = await CreateUser();
await Request(laterSuspended);
await using (var db = Db())
{
    var user = await db.Users.SingleAsync(u => u.Id == laterSuspended.Id);
    user.SoftDelete();
    await db.SaveChangesAsync();
}
Check(!await Reset(Token(laterSuspended.Email)), "Una cuenta suspendida después de recibir el token tampoco puede restablecer");
await using (var db = Db())
{
    var user = await db.Users.SingleAsync(u => u.Id == laterSuspended.Id);
    Check(user.DeletedAt is not null && hasher.Verify("old-password", user.Password), "El restablecimiento no reactiva ni modifica una cuenta suspendida");
}

var concurrent = await CreateUser();
await Request(concurrent);
var concurrentRaw = Token(concurrent.Email);
var attempts = await Task.WhenAll(Reset(concurrentRaw), Reset(concurrentRaw));
Check(attempts.Count(success => success) == 1, "Dos usos simultáneos del mismo token producen un único cambio");
var concurrentRequests = await CreateUser();
await Task.WhenAll(Request(concurrentRequests), Request(concurrentRequests));
Check(issuedTokens.Count(link => link.Email == concurrentRequests.Email) == 1, "Solicitudes simultáneas respetan el límite por cuenta");

foreach (var input in new[] {
    new ResetPasswordRequest { Token = "invalid", NewPassword = "123", ConfirmPassword = "different" },
    new ResetPasswordRequest { Token = new string('A', 43), NewPassword = "abcdef", ConfirmPassword = "different" }
})
{
    Check(!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(input,
        new System.ComponentModel.DataAnnotations.ValidationContext(input), [], true), "Se rechazan formato inválido, contraseña corta o confirmación distinta");
}
await using (var db = Db())
{
    var controller = new AuthController(Service(db), new TestEnvironment())
    { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    var unknownResponse = (OkObjectResult)await controller.ForgotPassword(new ForgotPasswordRequest { Email = "missing@example.com" });
    var suspendedResponse = (OkObjectResult)await controller.ForgotPassword(new ForgotPasswordRequest { Email = suspended.Email });
    Check(JsonSerializer.Serialize(unknownResponse.Value) == JsonSerializer.Serialize(suspendedResponse.Value), "El endpoint no revela si la cuenta existe o está suspendida");
}
var httpUser = await CreateUser();
var directTokenUser = await CreateUser();
await using (var db = Db())
{
    var controller = new AuthController(Service(db), new TestEnvironment())
    { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    var response = (OkObjectResult)await controller.ForgotPassword(new ForgotPasswordRequest { Email = directTokenUser.Email });
    using var body = JsonDocument.Parse(JsonSerializer.Serialize(response.Value));
    var token = body.RootElement.GetProperty("token").GetString()!;
    Check(token.Length == 43 && await Reset(token), "Development devuelve un token utilizable directamente sin correos");
}
await Request(httpUser);
await using (var db = Db())
{
    var controller = new AuthController(Service(db), new TestEnvironment())
    { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    var response = await controller.ResetPassword(new ResetPasswordRequest
    { Token = Token(httpUser.Email), NewPassword = "new-password", ConfirmPassword = "new-password" });
    Check(response is NoContentResult && controller.Response.Headers.SetCookie.ToString().Contains("01 Jan 1970"), "El endpoint devuelve 204 y borra la cookie sin iniciar sesión");
}
await using (var db = Db())
{
    var controller = new AuthController(Service(db), new TestEnvironment { EnvironmentName = Environments.Production })
    { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    Check(await controller.ForgotPassword(new ForgotPasswordRequest { Email = active.Email }) is NotFoundResult,
        "La entrega directa del token está deshabilitada en producción");
    Check(await controller.ResetPassword(new ResetPasswordRequest()) is NotFoundResult,
        "El restablecimiento de prueba está deshabilitado en producción");
}
await using (var db = Db())
{
    var service = Service(db);
    try
    {
        await service.LoginAsync(new LoginRequest { Email = active.Email, Password = "old-password" });
        throw new Exception("La contraseña anterior no debe permitir iniciar sesión.");
    }
    catch (UnauthorizedException)
    {
        Check(true, "AuthService rechaza el login con la contraseña anterior al restablecimiento");
    }
    var login = await service.LoginAsync(new LoginRequest { Email = active.Email, Password = "new-password" });
    Check(login.User.Id == active.Id && jwt.ValidateToken(login.AccessToken),
        "AuthService permite iniciar sesión con la nueva contraseña");
    var controller = new AuthController(service, new TestEnvironment())
    { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    var registered = await controller.Register(new RegisterRequest
    {
        FirstName = "Test", LastName = "User", Email = Guid.NewGuid() + "@example.com",
        Password = "register-password", Role = Domain.Enums.UserRoles.Customer
    });
    Check(registered is ObjectResult { StatusCode: 201 } && controller.Response.Headers.SetCookie.ToString().Contains("access_token="),
        "El registro sigue creando la sesión desde AuthController");
    Check(controller.Logout() is NoContentResult, "El logout sigue disponible en AuthController");
}
var rollbackResetUser = await CreateUser();
await Request(rollbackResetUser);
var rollbackResetToken = Token(rollbackResetUser.Email);
await using (var db = Db())
{
    var service = new AuthService(new UserRepository(db), jwt, hasher,
        new FailingTokenRepository(new PasswordResetTokenRepository(db), failOnCreate: false),
        clock, new TransactionManager(db));
    try
    {
        await service.ResetPasswordAsync(new ResetPasswordRequest
        { Token = rollbackResetToken, NewPassword = "new-password", ConfirmPassword = "new-password" });
        throw new Exception("Se esperaba el fallo de prueba.");
    }
    catch (InjectedFailureException)
    {
        Check(!db.ChangeTracker.Entries().Any() && await db.SaveChangesAsync() == 0,
            "El rollback limpia los cambios en memoria y evita que se guarden después");
    }
}
await using (var db = Db())
{
    var user = await db.Users.SingleAsync(user => user.Id == rollbackResetUser.Id);
    var tokenHash = Hash(rollbackResetToken);
    var token = await db.PasswordResetTokens.SingleAsync(token => token.TokenHash == tokenHash);
    Check(hasher.Verify("old-password", user.Password) && token.UsedAt is null,
        "Un fallo después de actualizar contraseña y tokens revierte ambas modificaciones");
}
Check(await Reset(rollbackResetToken), "El token sigue disponible para reintentar después del rollback");

var rollbackIssueUser = await CreateUser();
await Request(rollbackIssueUser);
var rollbackIssueToken = Token(rollbackIssueUser.Email);
clock.Advance(TimeSpan.FromSeconds(61));
await using (var db = Db())
{
    var service = new AuthService(new UserRepository(db), jwt, hasher,
        new FailingTokenRepository(new PasswordResetTokenRepository(db), failOnCreate: true),
        clock, new TransactionManager(db));
    try
    {
        await service.ForgotPasswordAsync(new ForgotPasswordRequest { Email = rollbackIssueUser.Email });
        throw new Exception("Se esperaba el fallo de prueba.");
    }
    catch (InjectedFailureException)
    {
        Check(!db.ChangeTracker.Entries().Any(), "Un fallo al generar un token también limpia el contexto");
    }
}
await using (var db = Db())
{
    var tokens = await db.PasswordResetTokens.Where(token => token.UserId == rollbackIssueUser.Id).ToListAsync();
    Check(tokens.Count == 1 && tokens[0].TokenHash == Hash(rollbackIssueToken) && tokens[0].UsedAt is null,
        "El rollback elimina el token nuevo y conserva válido el anterior");
}
Check(await Reset(rollbackIssueToken), "La solicitud fallida no inutiliza el token anterior");

var sameContextUser = await CreateUser();
await using (var db = Db())
{
    var service = Service(db);
    var token = await service.ForgotPasswordAsync(new ForgotPasswordRequest { Email = sameContextUser.Email });
    await service.ResetPasswordAsync(new ResetPasswordRequest
    { Token = token!, NewPassword = "new-password", ConfirmPassword = "new-password" });
    Check(hasher.Verify("new-password", (await db.Users.SingleAsync(user => user.Id == sameContextUser.Id)).Password),
        "Los repositorios comparten contexto sin conflictos al solicitar y restablecer");
}
Console.WriteLine($"{passed} comprobaciones exitosas.");

sealed class TestClock : TimeProvider
{
    private DateTimeOffset now = DateTimeOffset.UtcNow;
    public override DateTimeOffset GetUtcNow() => now;
    public void Advance(TimeSpan duration) => now += duration;
}

sealed class TestEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Development;
    public string ApplicationName { get; set; } = "PasswordResetChecks";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

sealed class InjectedFailureException : Exception { }

sealed class FailingTokenRepository(IPasswordResetTokenRepository inner, bool failOnCreate)
    : IPasswordResetTokenRepository
{
    public Task<PasswordResetToken?> GetByHashAsync(string hash) => inner.GetByHashAsync(hash);
    public Task<bool> HasRecentAsync(Guid userId, DateTime since) => inner.HasRecentAsync(userId, since);
    public async Task CreateAsync(PasswordResetToken token)
    {
        await inner.CreateAsync(token);
        if (failOnCreate) throw new InjectedFailureException();
    }
    public async Task InvalidateByUserAsync(Guid userId, DateTime now)
    {
        await inner.InvalidateByUserAsync(userId, now);
        if (!failOnCreate) throw new InjectedFailureException();
    }
}

