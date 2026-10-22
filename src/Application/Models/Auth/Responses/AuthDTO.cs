using Application.Models.Users.Responses;

namespace Application.Models.Auth.Responses;

public sealed record AuthDTO(
    string AccessToken,
    UserDTO User
);