using System.Text;

namespace Synapse.DeliveryRoutes.Application.Models;

public class DriverSchedule
{
    public required Driver Driver { get; set; }
    public required Vehicle Vehicle { get; set; }
    public required Order[] Orders { get; set; }
    public required GeoCoordinates StartLocation { get; set; }
    public required GeoCoordinates EndLocation { get; set; }
}
