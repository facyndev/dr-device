using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _applicationDbcontext;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext applicationDbcontext)
    {
        ArgumentNullException.ThrowIfNull(applicationDbcontext);
        _applicationDbcontext = applicationDbcontext;
        _dbSet = applicationDbcontext.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetById(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> Create(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Add(entity);
        await _applicationDbcontext.SaveChangesAsync();
        return entity;
    }

    public virtual async Task Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
        await _applicationDbcontext.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var entity = await GetById(id);
        if (entity is null)
        {
            return;
        }

        _dbSet.Remove(entity);
        await _applicationDbcontext.SaveChangesAsync();
    }
}
