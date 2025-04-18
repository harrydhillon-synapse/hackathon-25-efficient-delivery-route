namespace Synapse.DeliveryRoutes.Api.Models;

public class DeliveryRouteViewModel
{
    public required DriverViewModel Driver { get; set; }
    public required VehicleViewModel Vehicle { get; set; }
    public required OfficeViewModel Office { get; set; }
    public required RouteSummaryViewModel Summary { get; set; }
    public required List<DeliveryViewModel> Deliveries { get; set; }
}