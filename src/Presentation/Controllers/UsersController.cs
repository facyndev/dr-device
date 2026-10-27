using Application.Interfaces;
using Application.Models.Users.Requests;
using Application.Models.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(
    IUserService userService,
    ICurrentUserSession currentUserSession) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount()
    {
        if (!currentUserSession.IsAuthenticated)
            return Unauthorized();

        var user = await userService.GetAccountAsync(currentUserSession.UserId);
        Response.Headers.CacheControl = "no-store";
        return Ok(user);
    }

    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserSession.IsAuthenticated)
            return Unauthorized();

        await userService.UpdateAccountAsync(currentUserSession.UserId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAccount()
    {
        if (!currentUserSession.IsAuthenticated)
            return Unauthorized();

        await userService.DeleteAccountAsync(currentUserSession.UserId);
        return NoContent();
    }
}
