using Domain.Enums;
using Domain.Exceptions;

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

    protected Offer() { }

    public Offer(Guid technicianId, Guid orderId, decimal price, string description)
    {
        if (technicianId == Guid.Empty)
            throw new ValidationException("La oferta debe vincularse a un técnico.");
        if (orderId == Guid.Empty)
            throw new ValidationException("La oferta debe vincularse a una orden.");

        SetDetails(price, description);

        TechnicianId = technicianId;
        OrderId = orderId;
        Status = OfferStatus.Pending;
    }

    public void Update(decimal? price = null, string? description = null)
    {
        if (Status != OfferStatus.Pending)
            throw new ConflictException("Solo se puede modificar una oferta que se encuentra en estado pendiente.");

        SetDetails(price ?? Price, description ?? Description);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Accept()
    {
        if (Status != OfferStatus.Pending)
            throw new ConflictException("Solo se puede aceptar una oferta que se encuentra en estado pendiente.");

        Status = OfferStatus.Accepted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = OfferStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RestoreToPending()
    {
        Status = OfferStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelByTechnician(Guid technicianId)
    {
        if (TechnicianId != technicianId)
            throw new ForbiddenException("Solo el técnico que hizo la oferta puede cancelar su participación.");

        if (Status != OfferStatus.Accepted)
            throw new ConflictException("Solo se puede cancelar la participación en una oferta que había sido aceptada.");

        Status = OfferStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetDetails(decimal price, string description)
    {
        if (price <= 0)
            throw new ValidationException("El precio de la oferta debe ser mayor a cero.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("La descripción de la oferta no puede estar vacía.");

        Price = price;
        Description = description.Trim();
    }
}
