namespace Application.Models.Orders.Requests;

public record UpdateOrderRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}
