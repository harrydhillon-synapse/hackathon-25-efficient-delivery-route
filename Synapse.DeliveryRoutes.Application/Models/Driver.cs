namespace Synapse.DeliveryRoutes.Application.Models;

public class Driver
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    public List<CertificationType> Certifications { get; set; } = [];
    public List<DriverScheduleEntry> Schedule { get; set; } = [];
    public List<VehicleType> AllowedVehicles { get; set; } = [];
}

public class DriverScheduleEntry
{
    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }
}