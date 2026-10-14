using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth.Requests;

public record RegisterRequest
{
    [Required(ErrorMessage = "El nombre no puede estar vacío.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "El apellido no puede estar vacío.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El apellido debe tener entre 3 y 100 caracteres.")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico no puede estar vacío.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [MaxLength(255, ErrorMessage = "El correo electrónico no puede superar los 255 caracteres.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña no puede estar vacía.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
    public string Password { get; init; } = string.Empty;

    [StringLength(20, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 20 dígitos.")]
    public string? Phone { get; init; }

    [Required(ErrorMessage = "El rol no puede estar vacío.")]
    public Guid RoleId { get; init; }
}
