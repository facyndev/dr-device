using Application.Models.Users.Responses;

namespace Application.Interfaces;

public interface IUserService
{
    Task<UserDTO> GetAccountAsync(Guid userId);
    Task DeleteAccountAsync(Guid userId);
}
