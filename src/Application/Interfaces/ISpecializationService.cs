using Application.Models.Specializations.Requests;
using Application.Models.Specializations.Responses;

namespace Application.Interfaces;

public interface ISpecializationService
{
    Task<SpecializationsDTO> GetByTechnicianAsync(Guid userId);
    Task<SpecializationsDTO> ReplaceByTechnicianAsync(Guid userId, SetSpecializationsRequest request);
}
