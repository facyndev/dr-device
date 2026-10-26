using Application.Interfaces;
using Domain.Interfaces;

namespace Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
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
