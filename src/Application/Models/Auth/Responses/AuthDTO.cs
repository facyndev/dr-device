namespace Application.Models.Auth.Responses;

public record AuthDTO(
    string Token,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role
);
