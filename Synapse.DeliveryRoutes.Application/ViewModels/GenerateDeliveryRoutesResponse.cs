namespace Synapse.DeliveryRoutes.Application.ViewModels;

public class GenerateDeliveryRoutesResponse
{
    /// <summary>
    /// Indicates if route generation succeeded.
    /// </summary>
    public GenerateDeliveryRoutesResult Result { get; set; }

    /// <summary>
    /// The number of routes generated (null if failed).
    /// </summary>
    public int? RouteCount { get; set; }
}