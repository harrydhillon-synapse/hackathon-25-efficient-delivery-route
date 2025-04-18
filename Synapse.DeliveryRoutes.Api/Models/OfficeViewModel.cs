using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Api.Models;

public class OfficeViewModel
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinates Location { get; set; }
}