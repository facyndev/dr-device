namespace Application.Models.Categories.Responses;

public record CategoryDTO(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
