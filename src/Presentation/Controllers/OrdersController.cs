using Application.Interfaces;
using Application.Models.Orders.Requests;
using Application.Models.Orders.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(
    IOrderService orderService,
    ICurrentUserSession currentUserSession) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<OrderDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await orderService.GetAllAsync());
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IEnumerable<OrderDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMy()
    {
        return Ok(await orderService.GetMyOrdersAsync(currentUserSession.UserId));
    }

    [HttpGet("available")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(IEnumerable<OrderDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAvailable()
    {
        return Ok(await orderService.GetAvailableForTechnicianAsync(currentUserSession.UserId));
    }

    [HttpGet("assigned")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(IEnumerable<OrderDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssigned()
    {
        return Ok(await orderService.GetAssignedOrdersAsync(currentUserSession.UserId));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await orderService.GetByIdAsync(id, currentUserSession.UserId, currentUserSession.Role));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var result = await orderService.CreateAsync(currentUserSession.UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderRequest request)
    {
        return Ok(await orderService.UpdateAsync(id, currentUserSession.UserId, request));
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        return Ok(await orderService.CancelAsync(id, currentUserSession.UserId));
    }

    [HttpPatch("{id:guid}/finish")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Finish(Guid id)
    {
        return Ok(await orderService.FinishAsync(id, currentUserSession.UserId));
    }

    [HttpPatch("{id:guid}/technical-cancel")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(OrderDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelByTechnician(Guid id)
    {
        return Ok(await orderService.CancelByTechnicianAsync(id, currentUserSession.UserId));
    }
}
