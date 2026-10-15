using Application.Interfaces;
using Application.Models.Auth.Requests;
using Application.Models.Auth.Responses;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IHasher _hasher;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IHasher hasher)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _hasher = hasher;
    }

    public async Task<AuthDTO> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmail(request.Email);
        if (existing is not null)
            throw new Domain.Exceptions.ConflictException("Ya existe un usuario con ese correo electrónico.");

        var passwordHash = _hasher.Hash(request.Password);

        var user = new User(request.FirstName, request.LastName, request.Email, passwordHash, request.Phone);

        await _userRepository.Create(user);

        var token = _jwtService.GenerateToken(user, user.Role.ToString());
        return new AuthDTO(token);
    }

    public Task<AuthDTO> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        throw new NotImplementedException();
    }
}
