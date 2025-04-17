using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum VehicleType
{
    [Description("Car")]
    Car = 1,

    [Description("Truck")]
    Truck = 2
}