namespace Rest_API.Services.Locations;

public sealed class LocationDto
{
    public string Plz { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? State { get; set; }
}