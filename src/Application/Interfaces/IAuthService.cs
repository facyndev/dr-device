using Application.Models.Auth.Requests;
using Application.Models.Auth.Responses;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthDTO> LoginAsync(LoginRequest request);
    Task<AuthDTO> RegisterAsync(RegisterRequest request);
    Task LogoutAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

