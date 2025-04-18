using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.ViewModels;

public class OfficeViewModel
{
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinates Location { get; set; }
}