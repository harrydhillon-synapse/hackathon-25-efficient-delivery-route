namespace Synapse.DeliveryRoutes.Application.Models;

public class Vehicle
{
    public required string VehicleID { get; set; }
    public VehicleType Type { get; set; }
}