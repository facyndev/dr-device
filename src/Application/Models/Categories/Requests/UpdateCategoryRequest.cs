using System.ComponentModel.DataAnnotations;

namespace Application.Models.Categories.Requests;

public record UpdateCategoryRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}
