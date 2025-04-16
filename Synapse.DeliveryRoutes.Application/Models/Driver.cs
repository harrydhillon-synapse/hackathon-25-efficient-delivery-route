namespace Synapse.DeliveryRoutes.Application.Models;

public class Driver
{
    public required string DriverID { get; set; }
    public required string Name { get; set; }

    public List<CertificationType> Certifications { get; set; } = [];
    public Dictionary<Day, AvailabilityStatus> WeeklySchedule { get; set; } = [];
    public List<VehicleType> VehiclesCanDrive { get; set; } = [];
}