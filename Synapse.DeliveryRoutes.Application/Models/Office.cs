namespace Synapse.DeliveryRoutes.Application.Models;

public class Office
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinates Location { get; set; }
    public required OfficeContactInfo Contact { get; set; }
    public List<string> Facilities { get; set; } = [];
}

public class GeoCoordinates
{
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
}

public class OfficeContactInfo
{
    public required string Phone { get; set; }
    public required string Email { get; set; }
    public required string Hours { get; set; }
}
