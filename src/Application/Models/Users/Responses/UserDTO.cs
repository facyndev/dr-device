using Domain.Enums;
using Domain.Entities;

namespace Application.Models.Users.Responses;

public record UserDTO(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    UserRoles Role,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Address,
    Location? Location
);
