namespace Synapse.DeliveryRoutes.Application.ViewModels;

public class GenerateDeliveryRoutesResponse
{
    public required GenerateDeliveryRoutesResult Result { get; set; }
    public required int? RouteCount { get; set; }
}