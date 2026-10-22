using Domain.Enums;
using Domain.Exceptions;

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

    protected Order() { }

    public Order(string title, string description, Guid userId, Guid categoryId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("La orden debe vincularse a un usuario.");
        if (categoryId == Guid.Empty)
            throw new ValidationException("La orden debe vincularse a una categoría.");

        SetDetails(title, description);

        UserId = userId;
        CategoryId = categoryId;
        Status = OrderStatus.Searching;
    }

    public void Update(string? title = null, string? description = null, Guid? categoryId = null)
    {
        if (Status != OrderStatus.Searching)
            throw new ConflictException("Solo se puede modificar una orden que se encuentra en estado de búsqueda.");

        if (categoryId.HasValue)
        {
            if (categoryId.Value == Guid.Empty)
                throw new ValidationException("La orden debe vincularse a una categoría válida.");

            CategoryId = categoryId.Value;
        }

        SetDetails(title ?? Title, description ?? Description);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid userId)
    {
        if (UserId != userId)
            throw new ForbiddenException("Solo el cliente dueño de la orden puede cancelarla.");

        if (Status is OrderStatus.Cancelled or OrderStatus.Completed)
            throw new ConflictException("Una orden no puede ser cancelada si ya está cancelada o completada.");

        Status = OrderStatus.Cancelled;
        CanceledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Finish(Guid technicianId)
    {
        if (TechnicianId != technicianId)
            throw new ForbiddenException("Solo el técnico asignado a la orden puede finalizarla.");

        if (Status != OrderStatus.InProgress)
            throw new ConflictException("Una orden solo puede finalizarse si se encuentra en progreso.");

        Status = OrderStatus.Completed;
        FinishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTechnician(Guid technicianId)
    {
        if (technicianId == Guid.Empty)
            throw new ValidationException("El técnico asignado no es válido.");

        if (technicianId == UserId)
            throw new ConflictException("Un técnico no puede ser asignado a su propia orden.");

        if (TechnicianId.HasValue)
            throw new ConflictException("No se puede aceptar una oferta si la orden ya tiene un técnico asignado.");

        if (Status != OrderStatus.Searching)
            throw new ConflictException("Una oferta solo puede aceptarse si la orden sigue en estado de búsqueda.");

        TechnicianId = technicianId;
        Status = OrderStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelByTechnician(Guid technicianId)
    {
        if (TechnicianId != technicianId)
            throw new ForbiddenException("Solo el técnico asignado a la orden puede cancelar su participación.");

        if (Status != OrderStatus.InProgress)
            throw new ConflictException("Solo se puede realizar la cancelación técnica en una orden en curso.");

        TechnicianId = null;
        Status = OrderStatus.Searching;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetDetails(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("El título no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("La descripción no puede estar vacía.");

        Title = title.Trim();
        Description = description.Trim();
    }
}
