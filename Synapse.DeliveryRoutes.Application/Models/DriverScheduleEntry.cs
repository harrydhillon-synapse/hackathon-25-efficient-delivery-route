namespace Synapse.DeliveryRoutes.Application.Models;

public class DriverScheduleEntry
{
    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }
}