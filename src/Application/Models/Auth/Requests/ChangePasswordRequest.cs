using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth.Requests;

public record ChangePasswordRequest
{
    [Required(ErrorMessage = "La contraseña actual no puede estar vacía.")]
    [MinLength(6, ErrorMessage = "La contraseña actual debe tener mínimo 6 caracteres.")]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña no puede estar vacía.")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener mínimo 6 caracteres.")]
    public string NewPassword { get; init; } = string.Empty;
}
