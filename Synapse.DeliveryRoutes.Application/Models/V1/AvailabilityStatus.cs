using System.Text.Json.Serialization;

namespace Synapse.DeliveryRoutes.Application.Models.V1;

public enum AvailabilityStatus
{
    [JsonPropertyName("Unavailable")]
    Unavailable = 1,

    [JsonPropertyName("Available Morning")]
    AvailableMorning = 2,

    [JsonPropertyName("Available Evening")]
    AvailableEvening = 3,

    [JsonPropertyName("Available Full Day")]
    AvailableFullDay = 4
}