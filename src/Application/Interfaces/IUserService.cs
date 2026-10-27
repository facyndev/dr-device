using Application.Models.Users.Requests;
using Application.Models.Users.Responses;

namespace Application.Interfaces;

public interface IUserService
{
    Task<UserDTO> GetAccountAsync(Guid userId);
    Task UpdateAccountAsync(Guid userId, UpdateAccountRequest request, CancellationToken cancellationToken = default);
    Task DeleteAccountAsync(Guid userId);
}
