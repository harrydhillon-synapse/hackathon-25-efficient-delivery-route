using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class GetRoutingDataRequest
{
    public required GeoCoordinate OfficeCoordinate { get; set; }
    public required List<GeoCoordinate> DeliveryCoordinates { get; set; }
    public required int VehicleCount { get; set; }
}