using System.ComponentModel.DataAnnotations;

namespace Application.Models.Categories.Requests;

public record CreateCategoryRequest
{
    [Required(ErrorMessage = "El nombre no puede estar vacío.")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "La descripción no puede estar vacía.")]
    public string Description { get; init; } = string.Empty;
}
