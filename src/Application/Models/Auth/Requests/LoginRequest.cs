using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth.Requests;

public record LoginRequest
{
    [Required(ErrorMessage = "El correo electrónico no puede estar vacío.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [MaxLength(255, ErrorMessage = "El correo electrónico no puede superar los 255 caracteres.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña no puede estar vacía.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
    public string Password { get; init; } = string.Empty;
}
