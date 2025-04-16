namespace Synapse.DeliveryRoutes.Application.Models;

public class Product
{
    public required string ProductId { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required DetailedSpecifications DetailedSpecifications { get; set; }
    public required DeliveryRequirements DeliveryRequirements { get; set; }
}

public class DetailedSpecifications
{
    public required Dimensions Dimensions { get; set; }
}

public class Dimensions
{
    public required string Length { get; set; }
    public required string Width { get; set; }
    public required string Height { get; set; }
    public required string Weight { get; set; }
}

public class DeliveryRequirements
{
    public required string SpecialHandling { get; set; }
    public required string Packaging { get; set; }
    public required TransportationRequirements Transportation { get; set; }
}

public class TransportationRequirements
{
    public required bool TemperatureControlled { get; set; }
    public required string Orientation { get; set; }
}