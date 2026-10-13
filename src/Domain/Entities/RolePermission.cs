namespace Domain.Entities;

public class RolePermission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
}
