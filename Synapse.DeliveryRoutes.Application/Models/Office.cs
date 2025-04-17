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