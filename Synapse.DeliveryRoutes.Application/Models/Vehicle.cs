namespace Synapse.DeliveryRoutes.Application.Models;

public class Vehicle
{
    public required string VehicleId { get; set; }
    public required string Type { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required int Year { get; set; }
    public required string Capacity { get; set; }
    public List<string> Features { get; set; } = [];
}