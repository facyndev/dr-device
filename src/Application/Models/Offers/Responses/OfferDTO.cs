using Domain.Enums;

namespace Application.Models.Offers.Responses;

public record OfferDTO(
    Guid Id,
    Guid TechnicianId,
    Guid OrderId,
    OfferStatus Status,
    decimal Price,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
