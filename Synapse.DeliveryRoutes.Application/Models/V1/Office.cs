namespace Synapse.DeliveryRoutes.Application.Models;

public class Office
{
    public required string OfficeID { get; set; }
    public required string OfficeAddress { get; set; }
    public required GeoCoordinate OfficeGeocoordinates { get; set; }
}