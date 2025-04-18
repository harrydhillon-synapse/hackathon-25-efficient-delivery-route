namespace Synapse.DeliveryRoutes.Api.Models;

public class RouteSummaryViewModel
{
    public required double DistanceMiles { get; set; }
    public required string EstimateTimeOfReturnToBase { get; set; } // e.g. "17:30"
    public required int EfficiencyPercent { get; set; }
    public required int StopsCompleted { get; set; }
    public required int TotalStops { get; set; }
}