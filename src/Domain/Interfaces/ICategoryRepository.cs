using Domain.Entities;

namespace Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids);
}
