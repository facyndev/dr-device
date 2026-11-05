using Application.Interfaces;
using Application.Models.Offers.Requests;
using Application.Models.Offers.Responses;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITransactionManager _transactions;

    public OfferService(
        IOfferRepository offerRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        ITransactionManager transactions)
    {
        _offerRepository = offerRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _transactions = transactions;
    }

    public async Task<IEnumerable<OfferDTO>> GetByTechnicianAsync(Guid technicianId)
    {
        await EnsureUserExistsAsync(technicianId);
        var offers = await _offerRepository.GetByTechnicianIdAsync(technicianId);
        return offers.Select(ToDTO).ToList();
    }

    public async Task<IEnumerable<OfferDTO>> GetByOrderIdAsync(Guid orderId, Guid userId, UserRoles role)
    {
        await EnsureUserExistsAsync(userId);
        var order = await FindOrderAsync(orderId);

        if (role != UserRoles.Admin && order.UserId != userId)
            throw new ForbiddenException("Solo el cliente dueño de la orden puede consultar las ofertas realizadas sobre ella.");

        if (order.Status == OrderStatus.Cancelled)
            return [];

        var offers = await _offerRepository.GetByOrderIdAsync(orderId);
        return offers.Select(ToDTO).ToList();
    }

    public async Task<OfferDTO> GetByIdAsync(Guid id, Guid userId, UserRoles role)
    {
        await EnsureUserExistsAsync(userId);
        var offer = await FindOfferAsync(id);
        var order = await FindOrderAsync(offer.OrderId);

        if (role == UserRoles.Admin || offer.TechnicianId == userId)
            return ToDTO(offer);

        if (order.UserId == userId)
        {
            if (order.Status == OrderStatus.Cancelled)
                throw new NotFoundException("No se devuelven ofertas correspondientes a órdenes que hayan sido canceladas.");

            return ToDTO(offer);
        }

        throw new ForbiddenException("No tienes permiso para ver esta oferta.");
    }

    public async Task<OfferDTO> CreateAsync(Guid technicianId, CreateOfferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureUserExistsAsync(technicianId);

        var order = await FindOrderAsync(request.OrderId);

        if (order.UserId == technicianId)
            throw new ConflictException("Un técnico no puede ofertar a su propia orden.");

        if (order.TechnicianId.HasValue || await _offerRepository.HasAcceptedOfferAsync(order.Id))
            throw new ConflictException("No se puede ofertar a una orden que ya tiene una oferta aceptada.");

        if (order.Status != OrderStatus.Searching)
            throw new ConflictException("Un técnico solo puede ofertar a una orden si esta se encuentra en estado de búsqueda.");

        if (await _offerRepository.ExistsByOrderAndTechnicianAsync(order.Id, technicianId))
            throw new ConflictException("Un técnico no puede realizar más de una oferta para la misma orden.");

        var offer = new Offer(technicianId, order.Id, request.Price, request.Description);
        await _offerRepository.Create(offer);

        return ToDTO(offer);
    }

    public async Task<OfferDTO> UpdateAsync(Guid id, Guid technicianId, UpdateOfferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var offer = await FindOfferAsync(id);

        if (offer.TechnicianId != technicianId)
            throw new ForbiddenException("Solo el técnico que realizó la oferta puede modificarla.");

        offer.Update(request.Price, request.Description);
        await _offerRepository.Update(offer);

        return ToDTO(offer);
    }

    public async Task<OfferDTO> AcceptAsync(Guid id, Guid userId)
    {
        return await _transactions.ExecuteAsync(async () =>
        {
            var offer = await FindOfferAsync(id);
            var order = await FindOrderAsync(offer.OrderId);

            if (order.UserId != userId)
                throw new ForbiddenException("Solo el cliente dueño de la orden puede aceptar una oferta realizada sobre ella.");

            if (order.TechnicianId.HasValue || await _offerRepository.HasAcceptedOfferAsync(order.Id))
                throw new ConflictException("No se puede aceptar una oferta si la orden ya tiene un técnico asignado.");

            if (order.Status != OrderStatus.Searching)
                throw new ConflictException("Una oferta solo puede aceptarse si la orden sigue en estado de búsqueda.");

            var orderOffers = await _offerRepository.GetTrackedByOrderIdAsync(order.Id);

            offer.Accept();

            foreach (var otherOffer in orderOffers.Where(o => o.Id != offer.Id))
            {
                otherOffer.Reject();
            }

            order.AssignTechnician(offer.TechnicianId);
            await _orderRepository.Update(order);

            return ToDTO(offer);
        });
    }

    public async Task<OfferDTO> CancelByTechnicianAsync(Guid id, Guid technicianId)
    {
        return await _transactions.ExecuteAsync(async () =>
        {
            var offer = await FindOfferAsync(id);
            offer.CancelByTechnician(technicianId);

            var order = await FindOrderAsync(offer.OrderId);
            var orderOffers = await _offerRepository.GetTrackedByOrderIdAsync(order.Id);

            foreach (var otherOffer in orderOffers.Where(o => o.Id != offer.Id))
            {
                otherOffer.RestoreToPending();
            }

            order.CancelByTechnician(technicianId);
            await _orderRepository.Update(order);

            return ToDTO(offer);
        });
    }

    private async Task<Offer> FindOfferAsync(Guid id)
    {
        return await _offerRepository.GetById(id)
            ?? throw new NotFoundException("La oferta no existe.");
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

    private static OfferDTO ToDTO(Offer offer)
    {
        return new OfferDTO(
            offer.Id,
            offer.TechnicianId,
            offer.OrderId,
            offer.Status,
            offer.Price,
            offer.Description,
            offer.CreatedAt,
            offer.UpdatedAt);
    }
}
