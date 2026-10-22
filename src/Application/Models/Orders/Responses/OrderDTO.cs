using Domain.Enums;

namespace Application.Models.Orders.Responses;

public record OrderDTO(
    Guid Id,
    string Title,
    string Description,
    OrderStatus Status,
    Guid UserId,
    Guid? TechnicianId,
    Guid CategoryId,
    DateTime? FinishedAt,
    DateTime? CanceledAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
