using Application.Interfaces;
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
