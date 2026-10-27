using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Models.Auth.Requests;
using Application.Models.Users.Responses;
using Presentation.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IHostEnvironment environment) : ControllerBase
{
    [EnableRateLimiting("password-reset")]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var token = await authService.ForgotPasswordAsync(request);
        Response.Headers.CacheControl = "no-store";
        return Ok(new { token });
    }

    [EnableRateLimiting("password-reset")]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        await authService.ResetPasswordAsync(request);
        Response.Cookies.Delete(CookieConstants.AccessToken, CreateCookieOptions());
        Response.Headers.CacheControl = "no-store";
        return NoContent();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieConstants.AccessToken, CreateCookieOptions());
        Response.Headers.CacheControl = "no-store";
        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await authService.LoginAsync(request);
        Response.Cookies.Append(CookieConstants.AccessToken, user.AccessToken, CreateCookieOptions(TimeSpan.FromDays(1)));
        Response.Headers.CacheControl = "no-store";
        return Ok(user);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await authService.RegisterAsync(request);
        Response.Cookies.Append(CookieConstants.AccessToken, user.AccessToken, CreateCookieOptions(TimeSpan.FromDays(1)));
        Response.Headers.CacheControl = "no-store";
        return StatusCode(StatusCodes.Status201Created, user);
    }

    private CookieOptions CreateCookieOptions(TimeSpan? maxAge = null) => new()
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        SameSite = SameSiteMode.Lax,
        Path = "/",
        MaxAge = maxAge,
        Expires = maxAge.HasValue ? DateTimeOffset.UtcNow.Add(maxAge.Value) : null
    };
}
