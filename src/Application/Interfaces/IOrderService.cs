using Application.Models.Orders.Requests;
using Application.Models.Orders.Responses;
using Domain.Enums;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDTO>> GetAllAsync();
    Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(Guid userId);
    Task<IEnumerable<OrderDTO>> GetAssignedOrdersAsync(Guid technicianId);
    Task<IEnumerable<OrderDTO>> GetAvailableForTechnicianAsync(Guid technicianId);
    Task<OrderDTO> GetByIdAsync(Guid id, Guid userId, UserRoles role);
    Task<OrderDTO> CreateAsync(Guid userId, CreateOrderRequest request);
    Task<OrderDTO> UpdateAsync(Guid id, Guid userId, UpdateOrderRequest request);
    Task<OrderDTO> CancelAsync(Guid id, Guid userId);
    Task<OrderDTO> FinishAsync(Guid id, Guid technicianId);
    Task<OrderDTO> CancelByTechnicianAsync(Guid id, Guid technicianId);
}
