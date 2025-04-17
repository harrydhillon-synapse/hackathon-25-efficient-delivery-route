using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum CertificationType
{
    [Description("Basic")]
    Basic = 1,

    [Description("Respiratory")]
    Respiratory = 2,

    [Description("Mobility")]
    Mobility = 3,

    [Description("Hospital Beds")]
    HospitalBeds = 4,

    [Description("Complex")]
    Complex = 5
}