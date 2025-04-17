namespace Synapse.DeliveryRoutes.Application.Models;

public class Driver
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    public List<CertificationType> Certifications { get; set; } = [];
    public List<DriverScheduleEntry> Schedule { get; set; } = [];
    public List<VehicleType> AllowedVehicles { get; set; } = [];
}