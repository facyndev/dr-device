using Application.Interfaces;
using Domain.Interfaces;
using Application.Models.Users.Responses;
using Domain.Exceptions;

namespace Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
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
