namespace Application.Models.Offers.Requests;

public record UpdateOfferRequest
{
    public decimal? Price { get; init; }
    public string? Description { get; init; }
}
