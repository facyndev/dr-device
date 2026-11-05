using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth.Requests;

public record ResetPasswordRequest
{
    [Required]
    [StringLength(43, MinimumLength = 43)]
    [RegularExpression("^[A-Za-z0-9_-]{43}$")]
    public string Token { get; init; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener mínimo 6 caracteres.")]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
