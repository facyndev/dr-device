using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, string role, TimeSpan? expires = null);
    bool ValidateToken(string token);
}
