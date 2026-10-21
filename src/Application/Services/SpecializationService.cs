using Application.Interfaces;
using Application.Models.Specializations.Requests;
using Application.Models.Specializations.Responses;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public class SpecializationService : ISpecializationService
{
    private readonly ISpecializationRepository _specializations;
    private readonly IUserRepository _users;
    private readonly ICategoryRepository _categories;

    public SpecializationService(
        ISpecializationRepository specializations,
        IUserRepository users,
        ICategoryRepository categories)
    {
        _specializations = specializations;
        _users = users;
        _categories = categories;
    }

    public async Task<SpecializationsDTO> GetByTechnicianAsync(Guid userId)
    {
        await EnsureUserExistsAsync(userId);
        var categoryIds = await _specializations.GetCategoryIdsByUserAsync(userId);
        return new SpecializationsDTO(categoryIds);
    }

    public async Task<SpecializationsDTO> ReplaceByTechnicianAsync(Guid userId, SetSpecializationsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureUserExistsAsync(userId);

        var categoryIds = Normalize(request.CategoryIds);
        await EnsureCategoriesExistAsync(categoryIds);

        await _specializations.ReplaceForUserAsync(userId, categoryIds);
        return new SpecializationsDTO(categoryIds);
    }

    private async Task EnsureUserExistsAsync(Guid userId)
    {
        if (await _users.GetById(userId) is null)
            throw new NotFoundException("El usuario no existe.");
    }

    private async Task EnsureCategoriesExistAsync(IReadOnlyCollection<Guid> categoryIds)
    {
        if (categoryIds.Count == 0)
            return;

        if (!await _categories.AllExistAsync(categoryIds))
            throw new ValidationException("Una o más categorías seleccionadas no existen.");
    }

    private static IReadOnlyCollection<Guid> Normalize(IReadOnlyCollection<Guid> categoryIds)
    {
        ArgumentNullException.ThrowIfNull(categoryIds);

        var normalized = categoryIds.Distinct().ToArray();
        if (normalized.Any(id => id == Guid.Empty))
            throw new ValidationException("La selección contiene categorías inválidas.");

        return normalized;
    }
}
