namespace Synapse.DeliveryRoutes.Application.Models;

public class Product
{
    public required string ProductID { get; set; }
    
    public VehicleType RequiredVehicleType { get; set; }

    public CertificationType RequiredCertification { get; set; }

    public required string ProductName { get; set; }
}