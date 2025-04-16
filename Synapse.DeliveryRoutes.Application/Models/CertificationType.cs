using System.Text.Json.Serialization;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum CertificationType
{
    Basic = 1,
    Respiratory = 2,
    Mobility = 3,
    [JsonPropertyName("Hospital Beds")]
    HospitalBeds = 4,
    Complex = 5
}