using Application.Interfaces;
using Application.Models.Specializations.Requests;
using Application.Models.Specializations.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController(
    ISpecializationService specializationService,
    ICurrentUserSession currentUserSession) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(SpecializationsDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMy()
    {
        return Ok(await specializationService.GetByTechnicianAsync(currentUserSession.UserId));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(SpecializationsDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReplaceMy([FromBody] SetSpecializationsRequest request)
    {
        return Ok(await specializationService.ReplaceByTechnicianAsync(currentUserSession.UserId, request));
    }
}
