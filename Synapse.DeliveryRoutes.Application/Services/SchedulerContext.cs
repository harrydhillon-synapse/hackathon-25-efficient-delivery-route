using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class SchedulerContext
{
    private const int DepotIndex = 0;

    public SchedulerContext(SchedulingInputData inputData)
    {
        InputData = inputData;

        // === All Locations including Depot at 0 ===

        Locations = new List<GeoCoordinates> { inputData.Office.Location }
            .Concat(inputData.Orders.Select(o => o.Location))
            .ToArray();



        // === DRIVER-VEHICLE MATCHING (1-to-1) ===

        var certificationsRequiredForAllOrders = InputData.Orders
            .SelectMany(o => o.ProductIds)
            .Distinct()
            .Select(productId => InputData.Products.Single(p => p.Id == productId))
            .Select(p => p.DeliveryRequirements.Certification)
            .Distinct()
            .ToArray();

        // Create a list of vehicle-driver pairs where the driver can operate the vehicle
        // and the driver can handle at least 1 order
        var compatibleAssignments = (from driver in InputData.Drivers
            from vehicle in inputData.Vehicles
            where driver.AllowedVehicles.Contains(vehicle.Type)
                && driver.Certifications
                    .Any(c => certificationsRequiredForAllOrders.Contains(c))
            select new { driver, vehicle }).ToList();

        // Select a distinct 1-to-1 assignment (greedy)
        var assignedDriverIds = new HashSet<string>();
        var assignedVehicleIds = new HashSet<string>();

        foreach (var pair in compatibleAssignments)
        {
            if (assignedDriverIds.Contains(pair.driver.Id)
                || assignedVehicleIds.Contains(pair.vehicle.Id))
            {
                continue;
            }

            VehicleDriverAssignments.Add(new KeyValuePair<Vehicle, Driver>(pair.vehicle, pair.driver));
            assignedDriverIds.Add(pair.driver.Id);
            assignedVehicleIds.Add(pair.vehicle.Id);
        }



        // === Distances ===

        DistanceMatrix = new DistanceMatrix(LocationCount);
        Distances = DistanceMatrix.Build(Locations);



        // === Routing Manager and Model ===

        // Create the routing index manager
        RoutingIndexManager = new RoutingIndexManager(
            LocationCount,
            VehicleCount,
            DepotIndex);

        // Create the routing model
        RoutingModel = new RoutingModel(RoutingIndexManager);
    }
    public SchedulingInputData InputData { get; }
    public GeoCoordinates[] Locations { get; }
    public List<KeyValuePair<Vehicle, Driver>> VehicleDriverAssignments { get; set; } = [];
    public DistanceMatrix DistanceMatrix { get; }
    public double[,] Distances { get; }
    public RoutingIndexManager RoutingIndexManager { get; }
    public RoutingModel RoutingModel { get; }


    // Derived parameters
    public int LocationCount => Locations.Length;
    public int VehicleCount => VehicleDriverAssignments.Count;
}