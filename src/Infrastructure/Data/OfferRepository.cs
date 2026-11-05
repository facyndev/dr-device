using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class OfferRepository : Repository<Offer>, IOfferRepository
{
    public OfferRepository(ApplicationDbContext applicationDbcontext)
        : base(applicationDbcontext)
    {
    }

    public async Task<bool> ExistsByOrderAndTechnicianAsync(Guid orderId, Guid technicianId)
    {
        return await _dbSet.AnyAsync(o =>
            o.OrderId == orderId &&
            o.TechnicianId == technicianId);
    }

    public async Task<bool> HasAcceptedOfferAsync(Guid orderId)
    {
        return await _dbSet.AnyAsync(o =>
            o.OrderId == orderId &&
            o.Status == OfferStatus.Accepted);
    }

    public async Task<IEnumerable<Offer>> GetByTechnicianIdAsync(Guid technicianId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.TechnicianId == technicianId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Offer>> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(of => of.OrderId == orderId &&
                _applicationDbcontext.Orders.Any(ord => ord.Id == of.OrderId && ord.Status != OrderStatus.Cancelled))
            .OrderByDescending(of => of.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Offer>> GetTrackedByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Where(o => o.OrderId == orderId)
            .ToListAsync();
    }
}
