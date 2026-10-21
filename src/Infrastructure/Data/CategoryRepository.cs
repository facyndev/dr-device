using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext applicationDbcontext)
        : base(applicationDbcontext)
    {
    }

    public async Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids)
    {
        if (ids.Count == 0)
            return true;

        var distinct = ids.Distinct().ToList();
        var found = await _dbSet.CountAsync(category => distinct.Contains(category.Id));
        return found == distinct.Count;
    }
}
