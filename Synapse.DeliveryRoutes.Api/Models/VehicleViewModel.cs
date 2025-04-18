namespace Synapse.DeliveryRoutes.Api.Models;

public class VehicleViewModel
{
    public required string Id { get; set; }
    public required string Type { get; set; } // "Car" or "Truck"
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required int Year { get; set; }
}