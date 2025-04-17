using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum VehicleCapacity
{
    [Description("Small")]
    Small = 1,

    [Description("Medium")]
    Medium = 2,

    [Description("Large")]
    Large = 3
}