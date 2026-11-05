using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<int> CountActiveByUserAndCategoryAsync(Guid userId, Guid categoryId);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Order>> GetByTechnicianIdAsync(Guid technicianId);
    Task<IEnumerable<Order>> GetAvailableForTechnicianAsync(Guid technicianId);
    Task<bool> IsAvailableForTechnicianAsync(Guid orderId, Guid technicianId);
}
