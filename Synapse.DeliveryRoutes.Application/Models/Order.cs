using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public class Order
{
    public required string Id { get; set; }
    public required string PatientName { get; set; }
    public required string PatientPhone { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinates Location { get; set; }
    public required TimeWindow PreferredDeliveryTimeWindow { get; set; }
    public List<string> ProductIds { get; set; } = [];
    public DateOnly DeliveryDeadline { get; set; }
    public required OrderPriority Priority { get; set; }
    public List<string> Notes { get; set; } = [];
}



public enum OrderPriority
{
    [Description("Low")]
    Low = 1,

    [Description("Medium")]
    Medium = 2,

    [Description("High")]
    High = 3
}

public enum TimeWindow
{
    [Description("Morning")]
    Morning = 1,

    [Description("Afternoon")]
    Afternoon = 2
}