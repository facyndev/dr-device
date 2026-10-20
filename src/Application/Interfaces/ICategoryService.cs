using Application.Models.Categories.Requests;
using Application.Models.Categories.Responses;

namespace Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllAsync();
    Task<CategoryDTO> GetByIdAsync(Guid id);
    Task<CategoryDTO> CreateAsync(CreateCategoryRequest request);
    Task UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteAsync(Guid id);
}
