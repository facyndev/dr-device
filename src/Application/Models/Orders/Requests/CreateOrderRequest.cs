using System.ComponentModel.DataAnnotations;

namespace Application.Models.Orders.Requests;

public record CreateOrderRequest
{
    [Required(ErrorMessage = "El título no puede estar vacío.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "La descripción no puede estar vacía.")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "La categoría no puede estar vacía.")]
    public Guid CategoryId { get; init; }
}
