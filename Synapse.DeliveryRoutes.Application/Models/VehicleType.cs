using System.ComponentModel;

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
    [Description("Car")]
    Car = 1,

    [Description("Truck")]
    Truck = 2
}

public enum VehicleCapacity
{
    [Description("Small")]
    Small = 1,

    [Description("Medium")]
    Medium = 2,

    [Description("Large")]
    Large = 3
}

public enum VehicleFeature
{
    [Description("Climate Control")]
    ClimateControl = 1,

    [Description("GPS Tracking")]
    GPSTracking = 2,

    [Description("Safety Equipment")]
    SafetyEquipment = 3,

    [Description("Lift Gate")]
    LiftGate = 4
}

