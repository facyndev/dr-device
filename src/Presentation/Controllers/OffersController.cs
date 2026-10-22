using Application.Interfaces;
using Application.Models.Offers.Requests;
using Application.Models.Offers.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OffersController(
    IOfferService offerService,
    ICurrentUserSession currentUserSession) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(IEnumerable<OfferDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMy()
    {
        return Ok(await offerService.GetByTechnicianAsync(currentUserSession.UserId));
    }

    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OfferDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByOrderId(Guid orderId)
    {
        return Ok(await offerService.GetByOrderIdAsync(orderId, currentUserSession.UserId, currentUserSession.Role));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OfferDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await offerService.GetByIdAsync(id, currentUserSession.UserId, currentUserSession.Role));
    }

    [HttpPost]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(OfferDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateOfferRequest request)
    {
        var result = await offerService.CreateAsync(currentUserSession.UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(OfferDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOfferRequest request)
    {
        return Ok(await offerService.UpdateAsync(id, currentUserSession.UserId, request));
    }

    [HttpPatch("{id:guid}/accept")]
    [ProducesResponseType(typeof(OfferDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Accept(Guid id)
    {
        return Ok(await offerService.AcceptAsync(id, currentUserSession.UserId));
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(OfferDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelByTechnician(Guid id)
    {
        return Ok(await offerService.CancelByTechnicianAsync(id, currentUserSession.UserId));
    }
}
