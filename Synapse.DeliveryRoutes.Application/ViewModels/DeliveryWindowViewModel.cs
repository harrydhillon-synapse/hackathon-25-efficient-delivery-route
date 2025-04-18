namespace Synapse.DeliveryRoutes.Application.ViewModels;

public class DeliveryWindowViewModel
{
    public required string Start { get; set; } // e.g. "09:00"
    public required string End { get; set; }   // e.g. "09:30"
}