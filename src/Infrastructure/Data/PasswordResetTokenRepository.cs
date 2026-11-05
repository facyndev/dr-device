using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PasswordResetTokenRepository(ApplicationDbContext context) : IPasswordResetTokenRepository
{
    public Task<PasswordResetToken?> GetByHashAsync(string tokenHash) =>
        context.PasswordResetTokens.AsNoTracking().SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

    public Task<bool> HasRecentAsync(Guid userId, DateTime since) =>
        context.PasswordResetTokens.AnyAsync(token => token.UserId == userId && token.CreatedAt > since);

    public async Task CreateAsync(PasswordResetToken token)
    {
        context.PasswordResetTokens.Add(token);
        await context.SaveChangesAsync();
    }

    public async Task InvalidateByUserAsync(Guid userId, DateTime now) =>
        await context.PasswordResetTokens.Where(token => token.UserId == userId && token.UsedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.UsedAt, (DateTime?)now));
}
