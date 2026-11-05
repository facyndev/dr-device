using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmail(string email);
    Task<User?> GetByIdForUpdateAsync(Guid userId);
    Task<bool> HasOrdersOrOffersAsync(Guid userId);
    Task<int> DeleteExpiredBatchAsync(DateTime cutoffUtc, int batchSize, CancellationToken cancellationToken = default);
}
