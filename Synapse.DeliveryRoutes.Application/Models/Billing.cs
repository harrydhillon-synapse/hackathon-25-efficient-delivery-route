namespace Synapse.DeliveryRoutes.Application.Models;

public class Billing
{
    public decimal BasePrice { get; set; }
    public RentalRate RentalRate { get; set; } = new();
}