namespace Domain.Entities;

public record Location
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90
            || !double.IsFinite(longitude) || longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Las coordenadas no son válidas.");

        Latitude = latitude;
        Longitude = longitude;
    }
}
