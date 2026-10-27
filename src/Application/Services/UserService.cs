using Application.Interfaces;
using Domain.Interfaces;
using Application.Models.Users.Requests;
using Application.Models.Users.Responses;
using Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Application.Services;

public class UserService(IUserRepository userRepository, IAddressService addressService) : IUserService
{
    public async Task<UserDTO> GetAccountAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("El usuario autenticado no es válido.");

        var user = await userRepository.GetById(userId)
            ?? throw new NotFoundException("No se encontró la cuenta del usuario.");
        if (user.DeletedAt.HasValue)
            throw new UnauthorizedException("La cuenta fue eliminada. Iniciá sesión nuevamente.");

        return new UserDTO(user.Id, user.FirstName, user.LastName, user.Email, user.Phone,
            user.Role, user.CreatedAt, user.UpdatedAt, user.Address, user.Location);
    }

    public async Task UpdateAccountAsync(Guid userId, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedException("El usuario autenticado no es válido.");
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true))
            throw new Domain.Exceptions.ValidationException(string.Join(" ", errors.Select(error => error.ErrorMessage)));

        var user = await userRepository.GetById(userId)
            ?? throw new NotFoundException("No se encontró la cuenta del usuario.");
        if (user.DeletedAt.HasValue)
            throw new ConflictException("La cuenta ya fue eliminada.");

        var email = request.Email?.Trim() ?? user.Email;
        if (request.Email is not null)
        {
            var existing = await userRepository.GetByEmail(email);
            if (existing is not null && existing.Id != userId)
                throw new ConflictException("Ya existe un usuario con ese correo electrónico.");
        }

        if (request.Address is not null)
        {
            var address = await addressService.ValidateAsync(request.Address, request.Province!, request.Locality!, cancellationToken);
            user.SetAddress(address.Address, address.Location);
        }
        user.Update(request.FirstName?.Trim() ?? user.FirstName, request.LastName?.Trim() ?? user.LastName,
            email, request.PhoneProvided ? request.Phone?.Trim() : user.Phone, user.Role);
        await userRepository.Update(user);
    }

    public async Task DeleteAccountAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new Domain.Exceptions.UnauthorizedException("El usuario autenticado no es válido.");

        var user = await userRepository.GetById(userId) ?? throw new Domain.Exceptions.NotFoundException("No se encontró la cuenta del usuario.");

        if (user.DeletedAt.HasValue)
            throw new Domain.Exceptions.ConflictException("La cuenta ya fue eliminada.");

        if (await userRepository.HasOrdersOrOffersAsync(userId))
            throw new Domain.Exceptions.ConflictException(
                "No podés eliminar tu cuenta porque tenés órdenes u ofertas vinculadas.");

        user.SoftDelete();
        await userRepository.Update(user);
    }
}
