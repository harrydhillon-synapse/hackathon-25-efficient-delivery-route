namespace Synapse.DeliveryRoutes.Application.Models;

public class TransportRequirements
{
    public bool TemperatureControlled { get; set; }
    public required Orientation Orientation { get; set; }
    public required StackingLimit StackingLimit { get; set; }
}