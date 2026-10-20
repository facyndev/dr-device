using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public UserRoles Role { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; private set; }

    protected User() { }

    public User(string firstName, string lastName, string email, string passwordHash, string? phone = null, UserRoles role = UserRoles.Customer)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = passwordHash;
        Phone = phone;
        Role = role;
    }

    public void UpdatePassword(string passwordHash)
    {
        Password = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string firstName, string lastName, string email, string? phone, UserRoles role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

}
