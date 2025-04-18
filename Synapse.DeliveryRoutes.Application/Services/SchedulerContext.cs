using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class SchedulerContext
{
    public SchedulingInputData InputData { get; init; }
    public GeoCoordinates[] Locations { get; init; }
    public List<KeyValuePair<Vehicle, Driver>> VehicleDriverAssignments { get; init; } = [];
    public double[,] Distances { get; init; }
    public RoutingIndexManager RoutingIndexManager { get; init; }
    public RoutingModel RoutingModel { get; init; }


    // Derived parameters
    public int LocationCount => Locations.Length;
    public int VehicleCount => VehicleDriverAssignments.Count;
}