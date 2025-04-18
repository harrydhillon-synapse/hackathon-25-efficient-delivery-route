namespace Synapse.DeliveryRoutes.Application.ViewModels;

public class DeliveryProductViewModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool RequiresSetup { get; set; }
    public required int Quantity { get; set; }
    public required int ExpectedSetupTimeMinutes { get; set; }
    public required string Weight { get; set; } // e.g. "5.4 kg"
    public required string ImageUrl { get; set; }
}