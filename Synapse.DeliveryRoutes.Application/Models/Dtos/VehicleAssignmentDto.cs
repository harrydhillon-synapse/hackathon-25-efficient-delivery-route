using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Models.Dtos;

public class VehicleAssignmentDto
{
    public required Vehicle Vehicle { get; set; }
    public required List<Order> TransportableOrders { get; set; }
    public required List<Driver> AllowedDrivers { get; set; }
}