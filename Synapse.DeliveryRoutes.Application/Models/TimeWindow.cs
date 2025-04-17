using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum TimeWindow
{
    [Description("Morning")]
    Morning = 1,

    [Description("Afternoon")]
    Afternoon = 2
}