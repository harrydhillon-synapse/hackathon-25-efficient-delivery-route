namespace Synapse.DeliveryRoutes.Application.Models;

public class Order
{
    public required string OrderId { get; set; }
    public required string PatientName { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinate Coordinates { get; set; }
    public required string PreferredTimeWindow { get; set; }
    public List<string> ProductIds { get; set; } = [];
    public DateTime DeliveryDeadline { get; set; }
}