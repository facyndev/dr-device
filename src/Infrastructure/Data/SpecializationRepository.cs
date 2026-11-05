using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class SpecializationRepository(ApplicationDbContext context) : ISpecializationRepository
{
    public async Task<IReadOnlySet<Guid>> GetCategoryIdsByUserAsync(Guid userId)
    {
        var ids = await context.Specializations
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => s.CategoryId)
            .ToListAsync();

        return new HashSet<Guid>(ids);
    }

    public async Task ReplaceForUserAsync(Guid userId, IReadOnlyCollection<Guid> categoryIds)
    {
        await context.Specializations
            .Where(s => s.UserId == userId)
            .ExecuteDeleteAsync();

        foreach (var categoryId in categoryIds)
        {
            context.Specializations.Add(new Specialization(userId, categoryId));
        }

        await context.SaveChangesAsync();
    }
}
