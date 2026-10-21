using System.Security.Claims;
using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentUserSession : ICurrentUserSession
{
    private const string RawSubjectClaim = "sub";
    private const string RawRoleClaim = "role";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var value = FindClaim(ClaimTypes.NameIdentifier, RawSubjectClaim);
            if (value is null || !Guid.TryParse(value, out var userId))
                throw new UnauthorizedException("No hay una sesión activa.");

            return userId;
        }
    }

    public UserRoles Role
    {
        get
        {
            var value = FindClaim(ClaimTypes.Role, RawRoleClaim);
            if (value is null || !Enum.TryParse<UserRoles>(value, ignoreCase: true, out var role))
                throw new UnauthorizedException("No hay una sesión activa.");

            return role;
        }
    }

    private string? FindClaim(string mappedType, string rawType)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return null;

        return user.FindFirst(mappedType)?.Value
            ?? user.FindFirst(rawType)?.Value;
    }
}
