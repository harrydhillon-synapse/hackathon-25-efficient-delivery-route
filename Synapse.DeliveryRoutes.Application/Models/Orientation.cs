using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum Orientation
{
    [Description("Upright Position Preferred")]
    UprightPositionPreferred = 1,

    [Description("Upright Position")]
    UprightPosition = 2,

    [Description("Upright Position Required")]
    UprightPositionRequired = 3,

    [Description("Any Position")]
    AnyPosition = 4
}