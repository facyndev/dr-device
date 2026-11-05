using Domain.Entities;

namespace Domain.Interfaces;

public interface IOfferRepository : IRepository<Offer>
{
    Task<bool> ExistsByOrderAndTechnicianAsync(Guid orderId, Guid technicianId);
    Task<bool> HasAcceptedOfferAsync(Guid orderId);
    Task<IEnumerable<Offer>> GetByTechnicianIdAsync(Guid technicianId);
    Task<IEnumerable<Offer>> GetByOrderIdAsync(Guid orderId);
    Task<IReadOnlyList<Offer>> GetTrackedByOrderIdAsync(Guid orderId);
}
