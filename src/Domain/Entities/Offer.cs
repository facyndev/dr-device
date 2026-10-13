using Domain.Enums;

namespace Domain.Entities;

public class Offer
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TechnicianId { get; private set; }
    public Guid OrderId { get; private set; }
    public OfferStatus Status { get; private set; } = OfferStatus.Pending;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
}
