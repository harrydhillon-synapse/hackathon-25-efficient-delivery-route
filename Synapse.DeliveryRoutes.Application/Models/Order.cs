namespace Synapse.DeliveryRoutes.Application.Models;

public class Order
{
    public required string OrderId { get; set; }
    public required string PatientName { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinate GeoCoordinates { get; set; }
    public required string PreferredTimeWindow { get; set; }
    public List<string> ProductIds { get; set; } = [];
    public required string DeliveryDeadline { get; set; }
    public required string Priority { get; set; }
    public required DeliveryNotes DeliveryNotes { get; set; }
    public required OrderStatus Status { get; set; }
}

public class DeliveryNotes
{
    public required string Accessibility { get; set; }
    public required string SetupRequirements { get; set; }
    public required string SpecialInstructions { get; set; }
}

public class OrderStatus
{
    public required string CurrentStatus { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? Notes { get; set; }
}