using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Models.Dtos;

public class OrderAssignmentDto
{
    public required Order Order { get; set; }
    public required List<Vehicle> CompatibleVehicles { get; set; }
    public required List<Driver> CertifiedDrivers { get; set; }
}