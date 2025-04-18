using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Models.Dtos;

public class DriverAssignmentDto
{
    public required Driver Driver { get; set; }
    public required List<Vehicle> AllowedVehicles { get; set; }
    public required List<Order> CertifiedForOrders { get; set; }
}