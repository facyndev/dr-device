using System.ComponentModel.DataAnnotations;

namespace Application.Models.Offers.Requests;

public record CreateOfferRequest
{
    [Required(ErrorMessage = "La orden es obligatoria.")]
    public Guid OrderId { get; init; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    public decimal Price { get; init; }

    [Required(ErrorMessage = "La descripción no puede estar vacía.")]
    public string Description { get; init; } = string.Empty;
}
