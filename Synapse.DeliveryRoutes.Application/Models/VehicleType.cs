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

public enum VehicleType
{
    Car = 1,
    Truck = 2
}

public enum VehicleCapacity
{
    Small = 1,
    Medium = 2,
    Large = 3
}

public enum VehicleFeature
{
    ClimateControl = 1,
    GPSTracking = 2,
    SafetyEquipment = 3,
    LiftGate = 4
}

