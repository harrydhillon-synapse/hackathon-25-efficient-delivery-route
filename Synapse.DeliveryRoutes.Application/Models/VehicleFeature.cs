using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

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