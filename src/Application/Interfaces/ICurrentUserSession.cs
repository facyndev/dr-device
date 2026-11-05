using Domain.Enums;

namespace Application.Interfaces;

public interface ICurrentUserSession
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    UserRoles Role { get; }
}
