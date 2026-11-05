using Application.Models.Auth.Requests;
using Application.Models.Auth.Responses;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthDTO> LoginAsync(LoginRequest request);
    Task<AuthDTO> RegisterAsync(RegisterRequest request);
    Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

