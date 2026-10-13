namespace Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Action { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
}
