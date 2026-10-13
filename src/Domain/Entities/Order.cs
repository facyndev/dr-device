using Domain.Enums;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Searching; // por defecto una orden se crea en búsqueda
    public Guid UserId { get; private set; }
    public Guid? TechnicianId { get; private set; }
    public Guid CategoryId { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
}
