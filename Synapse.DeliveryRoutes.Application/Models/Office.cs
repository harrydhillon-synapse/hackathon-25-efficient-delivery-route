namespace Synapse.DeliveryRoutes.Application.Models;

public class Office
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinate GeoCoordinates { get; set; }
    public required ContactInfo Contact { get; set; }
    public List<string> Facilities { get; set; } = [];
}

public class ContactInfo
{
    public required string Phone { get; set; }
    public required string Email { get; set; }
    public required string Hours { get; set; }
}