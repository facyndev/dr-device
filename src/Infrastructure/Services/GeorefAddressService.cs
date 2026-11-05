using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Infrastructure.Services;

public sealed class GeorefAddressService(HttpClient client) : IAddressService
{
    public async Task<ValidatedAddress> ValidateAsync(string address, string province, string locality,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(province)
            || string.IsNullOrWhiteSpace(locality))
            throw new ValidationException("Ingresá dirección, provincia y localidad.");

        var query = $"direcciones?direccion={Uri.EscapeDataString(address.Trim())}"
            + $"&provincia={Uri.EscapeDataString(province.Trim())}"
            + $"&localidad={Uri.EscapeDataString(locality.Trim())}";

        try
        {
            using var response = await client.GetAsync(query, cancellationToken);
            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new ValidationException("La dirección, provincia o localidad ingresada no es válida.");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GeorefResponse>(cancellationToken);
            if (result?.Addresses is null)
                throw new JsonException("Respuesta de Georef incompleta.");
            if (result.Addresses.Length == 0)
                throw new ValidationException("No se encontró la dirección ingresada.");
            var match = result.Addresses[0];
            if (string.IsNullOrWhiteSpace(match.Name) || match.Name.Length > 500
                || match.Coordinates?.Latitude is not double lat
                || match.Coordinates.Longitude is not double lon
                || !double.IsFinite(lat) || lat is < -90 or > 90
                || !double.IsFinite(lon) || lon is < -180 or > 180)
                throw new ValidationException("No se pudo obtener una ubicación válida para la dirección ingresada.");

            return new ValidatedAddress(match.Name, new Location(lat, lon));
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException
            || ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new ExternalServiceException(
                "No se pudo consultar el servicio geográfico. Intentá registrarte nuevamente más tarde.", ex);
        }
    }

    private sealed record GeorefResponse(
        [property: JsonPropertyName("direcciones")] GeorefAddress[]? Addresses);

    private sealed record GeorefAddress(
        [property: JsonPropertyName("nomenclatura")] string? Name,
        [property: JsonPropertyName("ubicacion")] GeorefCoordinates? Coordinates);

    private sealed record GeorefCoordinates(
        [property: JsonPropertyName("lat")] double? Latitude,
        [property: JsonPropertyName("lon")] double? Longitude);
}
