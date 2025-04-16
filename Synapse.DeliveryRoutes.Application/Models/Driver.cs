namespace Synapse.DeliveryRoutes.Application.Models;

public class Driver
{
    public required string DriverId { get; set; }
    public required string Name { get; set; }
    public List<string> Certifications { get; set; } = [];
    public required WeeklySchedule WeeklySchedule { get; set; }
    public List<string> Vehicles { get; set; } = [];
}

public class WeeklySchedule
{
    public required string Monday { get; set; }
    public required string Tuesday { get; set; }
    public required string Wednesday { get; set; }
    public required string Thursday { get; set; }
    public required string Friday { get; set; }
    public required string Saturday { get; set; }
    public required string Sunday { get; set; }
}