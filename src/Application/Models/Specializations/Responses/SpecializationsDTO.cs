namespace Application.Models.Specializations.Responses;

public record SpecializationsDTO(
    IReadOnlyCollection<Guid> CategoryIds
);
