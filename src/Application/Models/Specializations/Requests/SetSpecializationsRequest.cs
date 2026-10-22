using System.ComponentModel.DataAnnotations;

namespace Application.Models.Specializations.Requests;

public record SetSpecializationsRequest
{
    [Required(ErrorMessage = "La lista de categorías no puede ser nula.")]
    public IReadOnlyCollection<Guid> CategoryIds { get; init; } = Array.Empty<Guid>();
}
