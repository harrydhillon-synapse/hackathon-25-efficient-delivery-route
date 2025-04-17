using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum StackingLimit
{
    [Description("1 Unit")]
    OneUnit = 1,

    [Description("2 Units")]
    TwoUnits = 2,

    [Description("3 Units")]
    ThreeUnits = 3,

    [Description("4 Units")]
    FourUnits = 4,

    [Description("5 Units")]
    FiveUnits = 5,

    [Description("Not Stackable")]
    NotStackable = 6
}