using Domain.Entities;

namespace Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmail(string email);
    Task<User?> GetByIdForUpdateAsync(Guid userId);
}
