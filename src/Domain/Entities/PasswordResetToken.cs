namespace Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    protected PasswordResetToken() { }

    public PasswordResetToken(Guid userId, string tokenHash, DateTime createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = createdAt.AddMinutes(10);
    }

    public bool IsValid(DateTime now) => UsedAt is null && now < ExpiresAt;
}
