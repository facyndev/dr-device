using Domain.Exceptions;

namespace Domain.Entities;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    protected Category() { }

    public Category(string name, string description)
    {
        SetDetails(name, description);
    }

    public void Update(string? name = null, string? description = null)
    {
        SetDetails(name ?? Name, description ?? Description);
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("El nombre no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("La descripción no puede estar vacía.");

        Name = name.Trim();
        Description = description.Trim();
    }
}
