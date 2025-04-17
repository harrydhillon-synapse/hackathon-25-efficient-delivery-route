using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Models.V1;

namespace Synapse.DeliveryRoutes.Application.Services;

public class SchedulerContext
{
    private const int DepotIndex = 0;

    public SchedulerContext(SchedulingInputData inputData)
    {
        InputData = inputData;

        // === All Locations including Depot at 0 ===

        Locations = new List<GeoCoordinate> { inputData.Office.OfficeGeocoordinates }
            .Concat(inputData.Orders.Select(o => o.Geocoordinates))
            .ToArray();



        // === DRIVER-VEHICLE MATCHING (1-to-1) ===

        // Create a list of vehicle-driver pairs where the driver can operate the vehicle
        var compatibleAssignments = (from driver in InputData.Drivers
            from vehicle in inputData.Vehicles
            where driver.VehiclesCanDrive.Contains(vehicle.Type)
            select new { driver, vehicle }).ToList();

        // Select a distinct 1-to-1 assignment (greedy)
        var assignedDriverIds = new HashSet<string>();
        var assignedVehicleIds = new HashSet<string>();

        foreach (var pair in compatibleAssignments)
        {
            if (assignedDriverIds.Contains(pair.driver.DriverID)
                || assignedVehicleIds.Contains(pair.vehicle.VehicleID))
            {
                continue;
            }

            VehicleDriverAssignments.Add(new KeyValuePair<Vehicle, Driver>(pair.vehicle, pair.driver));
            assignedDriverIds.Add(pair.driver.DriverID);
            assignedVehicleIds.Add(pair.vehicle.VehicleID);
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
    public GeoCoordinate[] Locations { get; }
    public List<KeyValuePair<Vehicle, Driver>> VehicleDriverAssignments { get; set; } = [];
    public DistanceMatrix DistanceMatrix { get; }
    public double[,] Distances { get; }
    public RoutingIndexManager RoutingIndexManager { get; }
    public RoutingModel RoutingModel { get; }


    // Derived parameters
    public int LocationCount => Locations.Length;
    public int VehicleCount => VehicleDriverAssignments.Count;
}