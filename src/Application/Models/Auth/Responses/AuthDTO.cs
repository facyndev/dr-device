namespace Application.Models.Auth.Responses;

public record AuthDTO(
    string Token,
    Guid UserId,
    string Email,
    string FullName,
    string Role
);
