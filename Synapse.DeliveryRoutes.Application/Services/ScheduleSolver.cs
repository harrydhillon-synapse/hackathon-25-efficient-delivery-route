using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;
using System.Text;

namespace Synapse.DeliveryRoutes.Application.Services;


public class SchedulingContext
{
    private const int DepotIndex = 0;

    public SchedulingContext(SchedulingInputData inputData)
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

public class ScheduleSolver
{
    public Result SolveSchedule(SchedulingContext schedulingContext)
    {
        RegisterDistanceCallback(schedulingContext);
        AddTimeDimension(schedulingContext);

        long fixedVehicleCost = 10000;
        for (int vehicleIdx = 0; vehicleIdx < schedulingContext.VehicleCount; vehicleIdx++)
        {
            schedulingContext.RoutingModel.SetFixedCostOfVehicle(fixedVehicleCost, vehicleIdx);
        }

        // Solver parameters
        var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = ScheduleSolverSettings.TimeLimitInSeconds };

        // Solve
        var solution = schedulingContext.RoutingModel.SolveWithParameters(searchParameters);

        if (solution == null)
        {
            return new Result { Successful = false, DriverSchedules = null };
        }

        var schedules = new List<DriverSchedule>();

        // Build results output
        var output = new StringBuilder();
        output.AppendLine("Solution found. Vehicle routes:");

        for (int vehicleIdx = 0; vehicleIdx < schedulingContext.VehicleCount; vehicleIdx++)
        {
            var keyValuePair = schedulingContext.VehicleDriverAssignments[vehicleIdx];
            var vehicle = keyValuePair.Key;
            var driver = keyValuePair.Value;

            if (!schedulingContext.RoutingModel.IsVehicleUsed(solution, vehicleIdx))
            {
                output.AppendLine($"Driver {driver.Name} ({driver.DriverID}) assigned to vehicle {vehicle.VehicleID} ({vehicle.Type}) - Not scheduled for any deliveries this day");
                continue;
            }

            var locations = new List<object>();
            var index = schedulingContext.RoutingModel.Start(vehicleIdx);
            while (!schedulingContext.RoutingModel.IsEnd(index))
            {
                var nodeIndex = schedulingContext.RoutingIndexManager.IndexToNode(index);
                //locations.Add($" -> {nodeIndex}");
                index = solution.Value(schedulingContext.RoutingModel.NextVar(index));
                if (nodeIndex == 0)
                {
                    locations.Add("Depot");
                }
                else
                {
                    locations.Add(schedulingContext.InputData.Orders[Convert.ToInt32(nodeIndex - 1)]);
                }
            }
            locations.Add("Depot");
            var deliveries = locations.Where(x => x is Order).OfType<Order>().ToArray();
            output.AppendLine($"Driver {driver.Name} ({driver.DriverID}) assigned to vehicle {vehicle.VehicleID} ({vehicle.Type}) - Scheduled for {deliveries.Length} deliveries this day");
            for (int deliveryIndex = 0; deliveryIndex < deliveries.Length; deliveryIndex++)
            {
                output.Append($"    {deliveryIndex + 1}) ");
                var order = deliveries[deliveryIndex];
                output.AppendLine($"Patient {order.PatientName} with {order.ProductIds.Count} products at: {order.Address}");
            }


            var schedule = new DriverSchedule
            {
                Orders = deliveries,
                Driver = driver,
                Vehicle = vehicle,
            };
            schedules.Add(schedule);
        }

        return new Result
        {
            Successful = true,
            DriverSchedules = schedules.ToArray(),
        };
    }

    /// <summary>
    /// Adds a time dimension to the routing model. This tracks cumulative travel time and enforces an 8-hour limit.
    /// </summary>
    private void AddTimeDimension(SchedulingContext schedulingContext)
    {
        // Register a callback to convert distance (km) to time (minutes) assuming 40 km/h average speed
        int transitCallbackIndex = schedulingContext.RoutingModel.RegisterTransitCallback((fromIndex, toIndex) =>
        {
            var fromNode = schedulingContext.RoutingIndexManager.IndexToNode(fromIndex);
            var toNode = schedulingContext.RoutingIndexManager.IndexToNode(toIndex);
            double distanceKm = schedulingContext.Distances[fromNode, toNode];
            double timeMinutes = distanceKm * 1.5; // 40 km/h → 1.5 minutes per km
            return Convert.ToInt32(timeMinutes);
        });

        // Add a time dimension with a max route duration of 480 minutes (8 hours)
        schedulingContext.RoutingModel.AddDimension(
            transitCallbackIndex,
            0,      // no slack (waiting time)
            ScheduleSolverSettings.MinutesPerWorkday,    // max total time per route
            true,   // force all routes to start at time zero
            "Time");

        var timeDimension = schedulingContext.RoutingModel.GetMutableDimension("Time");

        // Require each route to perform at least 1 minute of work
        for (int vehicleIdx = 0; vehicleIdx < schedulingContext.VehicleCount; vehicleIdx++)
        {
            var endIndex = schedulingContext.RoutingModel.End(vehicleIdx);
            timeDimension.CumulVar(endIndex).SetMin(1);
        }
    }

    /// <summary>
    /// Registers a callback function that returns the distance between two locations
    /// </summary>
    private void RegisterDistanceCallback(SchedulingContext schedulingContext)
    {
        // Create the distance callback
        // This callback returns the distance between two locations
        long DistanceCallback(long fromIndex, long toIndex)
        {
            // Convert from routing variable index to distance matrix index
            var fromNode = schedulingContext.RoutingIndexManager.IndexToNode(fromIndex);
            var toNode = schedulingContext.RoutingIndexManager.IndexToNode(toIndex);
            return Convert.ToInt64(schedulingContext.Distances[fromNode, toNode]);
        }

        // Register the distance callback
        var transitCallbackIndex = schedulingContext.RoutingModel.RegisterTransitCallback(DistanceCallback);
        schedulingContext.RoutingModel.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);
    }
}