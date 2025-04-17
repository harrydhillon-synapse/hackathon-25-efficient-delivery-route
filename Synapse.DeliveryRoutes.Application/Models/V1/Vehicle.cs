namespace Synapse.DeliveryRoutes.Application.Models.V1;

public class Vehicle
{
    public required string VehicleID { get; set; }
    public VehicleType Type { get; set; }
}