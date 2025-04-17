namespace Synapse.DeliveryRoutes.Application.Models;

public class Product
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required Dimensions Dimensions { get; set; }
    public required DeliveryRequirements DeliveryRequirements { get; set; }
    public required Billing Billing { get; set; }
}