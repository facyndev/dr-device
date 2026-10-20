using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.Models.Auth.Requests;
using Application.Models.Auth.Responses;
using Microsoft.AspNetCore.Authorization;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
