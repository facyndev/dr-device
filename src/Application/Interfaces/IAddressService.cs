using Domain.Entities;

namespace Application.Interfaces;

public record ValidatedAddress(string Address, Location Location);

public interface IAddressService
{
    Task<ValidatedAddress> ValidateAsync(string address, string province, string locality,
        CancellationToken cancellationToken = default);
}
