using Application.Interfaces;
using Application.Models.Categories.Requests;
using Application.Models.Categories.Responses;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAll();
        return categories.Select(ToDTO).ToList();
    }

    public async Task<CategoryDTO> GetByIdAsync(Guid id)
    {
        return ToDTO(await FindCategoryAsync(id));
    }

    public async Task<CategoryDTO> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category(request.Name, request.Description);
        await _categoryRepository.Create(category);
        return ToDTO(category);
    }

    public async Task UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await FindCategoryAsync(id);
        category.Update(request.Name, request.Description);
        await _categoryRepository.Update(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        await FindCategoryAsync(id);
        await _categoryRepository.Delete(id);
    }

    private async Task<Category> FindCategoryAsync(Guid id)
    {
        return await _categoryRepository.GetById(id)
            ?? throw new NotFoundException("La categoría no existe.");
    }

    private static CategoryDTO ToDTO(Category category)
    {
        return new CategoryDTO(category.Id, category.Name, category.Description,
            category.CreatedAt, category.UpdatedAt);
    }
}
