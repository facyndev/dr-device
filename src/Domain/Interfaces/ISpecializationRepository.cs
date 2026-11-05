using Domain.Entities;

namespace Domain.Interfaces;

public interface ISpecializationRepository
{
    Task<IReadOnlySet<Guid>> GetCategoryIdsByUserAsync(Guid userId);

    Task ReplaceForUserAsync(Guid userId, IReadOnlyCollection<Guid> categoryIds);
}
