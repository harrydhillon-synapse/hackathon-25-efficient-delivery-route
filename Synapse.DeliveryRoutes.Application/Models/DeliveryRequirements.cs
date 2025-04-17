namespace Synapse.DeliveryRoutes.Application.Models;

public class DeliveryRequirements
{
    public required string SpecialHandling { get; set; }
    public required PackagingType PackagingType { get; set; }
    public required TransportRequirements TransportRequirements { get; set; }
    public required SetupAssistanceLevel SetupAssistance { get; set; }
    public List<int> HistoricalSetupTimes { get; set; } = [];
}