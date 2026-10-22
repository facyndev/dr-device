using Application.Interfaces;
using Application.Models.Orders.Requests;
using Application.Models.Orders.Responses;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public class OrderService : IOrderService
{
    private const int MaxActiveOrdersPerCategory = 5;

    private readonly IOrderRepository _orderRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITransactionManager _transactions;

    public OrderService(
        IOrderRepository orderRepository,
        IOfferRepository offerRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository,
        ITransactionManager transactions)
    {
        _orderRepository = orderRepository;
        _offerRepository = offerRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _transactions = transactions;
    }

    public async Task<IEnumerable<OrderDTO>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAll();
        return orders.Select(ToDTO).ToList();
    }

    public async Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(Guid userId)
    {
        await EnsureUserExistsAsync(userId);
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(ToDTO).ToList();
    }

    public async Task<IEnumerable<OrderDTO>> GetAssignedOrdersAsync(Guid technicianId)
    {
        await EnsureUserExistsAsync(technicianId);
        var orders = await _orderRepository.GetByTechnicianIdAsync(technicianId);
        return orders.Select(ToDTO).ToList();
    }

    public async Task<IEnumerable<OrderDTO>> GetAvailableForTechnicianAsync(Guid technicianId)
    {
        await EnsureUserExistsAsync(technicianId);
        var orders = await _orderRepository.GetAvailableForTechnicianAsync(technicianId);
        return orders.Select(ToDTO).ToList();
    }

    public async Task<OrderDTO> GetByIdAsync(Guid id, Guid userId, UserRoles role)
    {
        var order = await FindOrderAsync(id);

        if (role == UserRoles.Admin || order.UserId == userId || order.TechnicianId == userId)
            return ToDTO(order);

        if (role == UserRoles.Technician && await _orderRepository.IsAvailableForTechnicianAsync(id, userId))
            return ToDTO(order);

        throw new ForbiddenException("No tienes permiso para ver esta orden.");
    }

    public async Task<OrderDTO> CreateAsync(Guid userId, CreateOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureUserExistsAsync(userId);
        await EnsureCategoryExistsAsync(request.CategoryId);
        await EnsureActiveOrderLimitNotExceededAsync(userId, request.CategoryId);

        var order = new Order(request.Title, request.Description, userId, request.CategoryId);
        await _orderRepository.Create(order);

        return ToDTO(order);
    }

    public async Task<OrderDTO> UpdateAsync(Guid id, Guid userId, UpdateOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var order = await FindOrderAsync(id);

        if (order.UserId != userId)
            throw new ForbiddenException("Solo el cliente dueño de la orden puede modificarla.");

        if (request.CategoryId.HasValue && request.CategoryId.Value != order.CategoryId)
        {
            await EnsureCategoryExistsAsync(request.CategoryId.Value);
            await EnsureActiveOrderLimitNotExceededAsync(userId, request.CategoryId.Value);
        }

        order.Update(request.Title, request.Description, request.CategoryId);
        await _orderRepository.Update(order);

        return ToDTO(order);
    }

    public async Task<OrderDTO> CancelAsync(Guid id, Guid userId)
    {
        var order = await FindOrderAsync(id);
        order.Cancel(userId);
        await _orderRepository.Update(order);

        return ToDTO(order);
    }

    public async Task<OrderDTO> FinishAsync(Guid id, Guid technicianId)
    {
        var order = await FindOrderAsync(id);
        order.Finish(technicianId);
        await _orderRepository.Update(order);

        return ToDTO(order);
    }

    public async Task<OrderDTO> CancelByTechnicianAsync(Guid id, Guid technicianId)
    {
        return await _transactions.ExecuteAsync(async () =>
        {
            var order = await FindOrderAsync(id);
            order.CancelByTechnician(technicianId);

            var orderOffers = await _offerRepository.GetTrackedByOrderIdAsync(order.Id);
            foreach (var offer in orderOffers)
            {
                if (offer.TechnicianId == technicianId && offer.Status == OfferStatus.Accepted)
                {
                    offer.Reject();
                }
                else
                {
                    offer.RestoreToPending();
                }
            }

            await _orderRepository.Update(order);
            return ToDTO(order);
        });
    }

    private async Task<Order> FindOrderAsync(Guid id)
    {
        return await _orderRepository.GetById(id)
            ?? throw new NotFoundException("La orden no existe.");
    }

    private async Task EnsureUserExistsAsync(Guid userId)
    {
        if (await _userRepository.GetById(userId) is null)
            throw new NotFoundException("El usuario no existe.");
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId)
    {
        if (await _categoryRepository.GetById(categoryId) is null)
            throw new NotFoundException("La categoría no existe.");
    }

    private async Task EnsureActiveOrderLimitNotExceededAsync(Guid userId, Guid categoryId)
    {
        var activeCount = await _orderRepository.CountActiveByUserAndCategoryAsync(userId, categoryId);
        if (activeCount >= MaxActiveOrdersPerCategory)
            throw new ConflictException($"No se pueden tener más de {MaxActiveOrdersPerCategory} órdenes activas (en búsqueda o pendientes) por categoría.");
    }

    private static OrderDTO ToDTO(Order order)
    {
        return new OrderDTO(
            order.Id,
            order.Title,
            order.Description,
            order.Status,
            order.UserId,
            order.TechnicianId,
            order.CategoryId,
            order.FinishedAt,
            order.CanceledAt,
            order.CreatedAt,
            order.UpdatedAt);
    }
}
