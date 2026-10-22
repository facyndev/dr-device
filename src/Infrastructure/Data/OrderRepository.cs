using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext applicationDbcontext)
        : base(applicationDbcontext)
    {
    }

    public async Task<int> CountActiveByUserAndCategoryAsync(Guid userId, Guid categoryId)
    {
        return await _dbSet.CountAsync(o =>
            o.UserId == userId &&
            o.CategoryId == categoryId &&
            (o.Status == OrderStatus.Searching || o.Status == OrderStatus.Pending));
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByTechnicianIdAsync(Guid technicianId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.TechnicianId == technicianId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAvailableForTechnicianAsync(Guid technicianId)
    {
        return await QueryAvailableForTechnician(technicianId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsAvailableForTechnicianAsync(Guid orderId, Guid technicianId)
    {
        return await QueryAvailableForTechnician(technicianId)
            .AnyAsync(o => o.Id == orderId);
    }

    private IQueryable<Order> QueryAvailableForTechnician(Guid technicianId)
    {
        var specializedCategoryIds = _applicationDbcontext.Specializations
            .Where(s => s.UserId == technicianId)
            .Select(s => s.CategoryId);

        var offeredOrderIds = _applicationDbcontext.Offers
            .Where(of => of.TechnicianId == technicianId)
            .Select(of => of.OrderId);

        return _dbSet
            .AsNoTracking()
            .Where(o =>
                o.Status == OrderStatus.Searching &&
                o.UserId != technicianId &&
                specializedCategoryIds.Contains(o.CategoryId) &&
                !offeredOrderIds.Contains(o.Id));
    }
}
