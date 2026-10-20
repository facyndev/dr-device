using Domain.Entities;

namespace Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByHashAsync(string tokenHash);
    Task<bool> HasRecentAsync(Guid userId, DateTime since);
    Task CreateAsync(PasswordResetToken token);
    Task InvalidateByUserAsync(Guid userId, DateTime now);
}
