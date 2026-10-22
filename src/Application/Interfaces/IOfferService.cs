using Application.Models.Offers.Requests;
using Application.Models.Offers.Responses;
using Domain.Enums;

namespace Application.Interfaces;

public interface IOfferService
{
    Task<IEnumerable<OfferDTO>> GetByTechnicianAsync(Guid technicianId);
    Task<IEnumerable<OfferDTO>> GetByOrderIdAsync(Guid orderId, Guid userId, UserRoles role);
    Task<OfferDTO> GetByIdAsync(Guid id, Guid userId, UserRoles role);
    Task<OfferDTO> CreateAsync(Guid technicianId, CreateOfferRequest request);
    Task<OfferDTO> UpdateAsync(Guid id, Guid technicianId, UpdateOfferRequest request);
    Task<OfferDTO> AcceptAsync(Guid id, Guid userId);
    Task<OfferDTO> CancelByTechnicianAsync(Guid id, Guid technicianId);
}
