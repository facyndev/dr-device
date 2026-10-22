using System.ComponentModel.DataAnnotations;

namespace Application.Models.Auth.Requests;

public record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; init; } = string.Empty;
}
