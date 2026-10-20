using Application.Interfaces;
using Application.Models.Auth.Requests;
using Application.Models.Auth.Responses;
using Application.Models.Users.Responses;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IHasher _hasher;
    private readonly IPasswordResetTokenRepository _passwordResetTokens;
    private readonly TimeProvider _clock;
    private readonly ITransactionManager _transactions;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IHasher hasher,
        IPasswordResetTokenRepository passwordResetTokens, TimeProvider clock, ITransactionManager transactions)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _hasher = hasher;
        _passwordResetTokens = passwordResetTokens;
        _clock = clock;
        _transactions = transactions;
    }

    public async Task<AuthDTO> RegisterAsync(RegisterRequest request)
    {
        var targetRole = request.Role ?? UserRoles.Customer;

        if (targetRole != UserRoles.Customer && targetRole != UserRoles.Technician)
            throw new Domain.Exceptions.ValidationException("El rol ingresado no es válido.");

        var existing = await _userRepository.GetByEmail(request.Email);
        if (existing is not null)
            throw new Domain.Exceptions.ConflictException("Ya existe un usuario con ese correo electrónico.");

        var passwordHash = _hasher.Hash(request.Password);

        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            request.Phone,
            targetRole);

        await _userRepository.Create(user);

        var token = _jwtService.GenerateToken(user, user.Role.ToString());
        
        var userDTO = new UserDTO(
            user.Id, user.FirstName, user.LastName, user.Email, user.Phone,
            user.Role, user.CreatedAt, user.UpdatedAt);

        return new AuthDTO(
            token,
            userDTO
        );    
    }

    public async Task<AuthDTO> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = await _userRepository.GetByEmail(request.Email) ?? 
            throw new Domain.Exceptions.UnauthorizedException("Las credenciales no coinciden.");

        if (!_hasher.Verify(request.Password, user.Password))
            throw new Domain.Exceptions.UnauthorizedException("Las credenciales no coinciden.");

        if (user.DeletedAt is not null)
        {
            user.Restore();
            await _userRepository.Update(user);
        }

        var token = _jwtService.GenerateToken(user, user.Role.ToString(), TimeSpan.FromDays(1));
        
        var userDTO = new UserDTO(
            user.Id, user.FirstName, user.LastName, user.Email, user.Phone,
            user.Role, user.CreatedAt, user.UpdatedAt);

        return new AuthDTO(
            token,
            userDTO
        );    
    }

    public async Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return await _transactions.ExecuteAsync<string?>(async () =>
        {
            var user = await _userRepository.GetByEmail(request.Email);
            if (user is null)
                return null;

            user = await _userRepository.GetByIdForUpdateAsync(user.Id);
            if (user is null || user.DeletedAt is not null)
                return null;

            var now = _clock.GetUtcNow().UtcDateTime;
            if (await _passwordResetTokens.HasRecentAsync(user.Id, now.AddMinutes(-1)))
                return null;

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var resetToken = new PasswordResetToken(user.Id, HashToken(token), now);
            await _passwordResetTokens.InvalidateByUserAsync(user.Id, now);
            await _passwordResetTokens.CreateAsync(resetToken);
            return token;
        });
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var passwordHash = _hasher.Hash(request.NewPassword);
        var tokenHash = HashToken(request.Token);
        await _transactions.ExecuteAsync(async () =>
        {
            var token = await _passwordResetTokens.GetByHashAsync(tokenHash);
            var user = token is null ? null : await _userRepository.GetByIdForUpdateAsync(token.UserId);
            if (user is null || user.DeletedAt is not null)
                throw new Domain.Exceptions.ValidationException("El token de restablecimiento no es válido o ha vencido.");

            // Otro intento pudo consumir el token mientras se esperaba el bloqueo del usuario.
            token = await _passwordResetTokens.GetByHashAsync(tokenHash);
            var now = _clock.GetUtcNow().UtcDateTime;
            if (token is null || !token.IsValid(now))
                throw new Domain.Exceptions.ValidationException("El token de restablecimiento no es válido o ha vencido.");

            user.UpdatePassword(passwordHash);
            await _userRepository.Update(user);
            await _passwordResetTokens.InvalidateByUserAsync(user.Id, now);
        });
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        throw new NotImplementedException();
    }
}
