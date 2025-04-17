namespace Synapse.DeliveryRoutes.Application.Models.V1;

public class Driver
{
    public required string DriverID { get; set; }
    public required string Name { get; set; }

    public List<CertificationType> Certifications { get; set; } = [];
    public KeyValuePair<Day, AvailabilityStatus>[] WeeklySchedule { get; set; } = [];
    public List<VehicleType> VehiclesCanDrive { get; set; } = [];
}