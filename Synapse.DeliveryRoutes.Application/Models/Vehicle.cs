namespace Synapse.DeliveryRoutes.Application.Models;

public class Vehicle
{
    public required string Id { get; set; }
    public required VehicleType Type { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required int Year { get; set; }
    public required VehicleCapacity Capacity { get; set; }
    public List<VehicleFeature> Features { get; set; } = [];
}