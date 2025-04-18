using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class Scheduler
{
    private const int DepotIndex = 0;
    private readonly SchedulerContext _schedulerContext;

    public Scheduler(SchedulingInputData inputData)
    {
        // === All Locations including Depot at 0 ===

        var locations = new List<GeoCoordinates> { inputData.Office.Location }
            .Concat(inputData.Orders.Select(o => o.Location))
            .ToArray();



        // === DRIVER-VEHICLE MATCHING (1-to-1) ===

        var vehicleDriverAssignments = new List<KeyValuePair<Vehicle, Driver>>();

        var certificationsRequiredForAllOrders = inputData.Orders
            .SelectMany(o => o.ProductIds)
            .Distinct()
            .Select(productId => inputData.Products.Single(p => p.Id == productId))
            .Select(p => p.DeliveryRequirements.Certification)
            .Distinct()
            .ToArray();

        // Create a list of vehicle-driver pairs where the driver can operate the vehicle
        // and the driver can handle at least 1 order
        var compatibleAssignments = (from driver in inputData.Drivers
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
            var driver = pair.driver;
            var vehicle = pair.vehicle;

            // Already matched?
            if (assignedDriverIds.Contains(driver.Id) || assignedVehicleIds.Contains(vehicle.Id))
            {
                continue;
            }

            var driverCerts = driver.Certifications.ToHashSet();
            var canHandleAnyOrder = inputData.Orders.Any(order =>
            {
                var requiredCerts = inputData.Products
                    .Where(p => order.ProductIds.Contains(p.Id))
                    .Select(p => p.DeliveryRequirements.Certification)
                    .ToHashSet();

                var requiredVehicleTypes = inputData.Products
                    .Where(p => order.ProductIds.Contains(p.Id))
                    .SelectMany(p => p.DeliveryRequirements.TransportRequirements.VehicleTypes)
                    .ToHashSet();

                return requiredCerts.All(rc => driverCerts.Contains(rc)) &&
                       requiredVehicleTypes.Contains(vehicle.Type);
            });

            if (!canHandleAnyOrder)
                continue;

            vehicleDriverAssignments.Add(new KeyValuePair<Vehicle, Driver>(vehicle, driver));
            assignedDriverIds.Add(driver.Id);
            assignedVehicleIds.Add(vehicle.Id);
        }



        // === Distances ===

        var distances = Utilities.BuildDistanceMatrix(locations);



        // === Routing Manager and Model ===

        // Create the routing index manager
        var routingIndexManager = new RoutingIndexManager(
            locations.Length,
            vehicleDriverAssignments.Count,
            DepotIndex);

        // Create the routing model
        var routingModel = new RoutingModel(routingIndexManager);

        _schedulerContext = new SchedulerContext
        {
            RoutingModel = routingModel,
            RoutingIndexManager = routingIndexManager,
            Distances = distances,
            InputData = inputData,
            Locations = locations,
            VehicleDriverAssignments = vehicleDriverAssignments
        };
    }

    public Schedule CreateSchedule()
    {
        RegisterDistanceCallback();
        AddTimeDimension();
        ApplyDriverCertificationConstraints();
        PreferToUseAllVehicles();

        // Solver parameters
        var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = Settings.TimeLimitInSeconds };
        searchParameters.LogSearch = true;

        // Solve
        var solution = _schedulerContext.RoutingModel.SolveWithParameters(searchParameters);

        return ProcessSolution(solution);
    }

    private void ApplyDriverCertificationConstraints()
    {
        var orderToAllowedVehicles = GetOrderNodeToAllowedVehiclesMap();
        foreach (var kvp in orderToAllowedVehicles)
        {
            int nodeIndex = kvp.Key; // This is the real-world node index (e.g., 1 for first order)
            var allowedVehiclesIndices = kvp.Value;

            // Convert real-world node index to internal OR-Tools routing index
            var routingIndex = _schedulerContext.RoutingIndexManager.NodeToIndex(nodeIndex);
            _schedulerContext.RoutingModel.SetAllowedVehiclesForIndex(allowedVehiclesIndices, routingIndex);
        }
    }

    private Dictionary<int, int[]> GetOrderNodeToAllowedVehiclesMap()
    {
        var map = new Dictionary<int, int[]>();

        for (int orderIdx = 0; orderIdx < _schedulerContext.InputData.Orders.Count; orderIdx++)
        {
            var order = _schedulerContext.InputData.Orders[orderIdx];
            var requiredCerts = _schedulerContext.InputData.Products
                .Where(p => order.ProductIds.Contains(p.Id))
                .Select(p => p.DeliveryRequirements.Certification)
                .Distinct()
                .ToHashSet();

            // Get all vehicle types that are acceptable for this order
            var acceptableVehicleTypes = _schedulerContext.InputData.Products
                .Where(p => order.ProductIds.Contains(p.Id))
                .SelectMany(p => p.DeliveryRequirements.TransportRequirements.VehicleTypes)
                .Distinct()
                .ToHashSet();

            var allowedVehicleIndices = new List<int>();

            for (int vehicleIdx = 0; vehicleIdx < _schedulerContext.VehicleDriverAssignments.Count; vehicleIdx++)
            {
                var vehicle = _schedulerContext.VehicleDriverAssignments[vehicleIdx].Key;
                var driver = _schedulerContext.VehicleDriverAssignments[vehicleIdx].Value;
                var driverCerts = driver.Certifications.ToHashSet();

                bool driverIsCertified = requiredCerts.All(rc => driverCerts.Contains(rc));
                bool vehicleTypeIsCompatible = acceptableVehicleTypes.Contains(vehicle.Type);

                if (driverIsCertified && vehicleTypeIsCompatible)
                {
                    allowedVehicleIndices.Add(vehicleIdx);
                }
            }

            map[orderIdx + 1] = allowedVehicleIndices.ToArray(); // +1 because node 0 is the depot
        }

        return map;
    }

    private Schedule ProcessSolution(Assignment? solution)
    {
        if (solution == null)
        {
            return new Schedule { Successful = false, DriverSchedules = null };
        }

        var schedules = new List<DriverSchedule>();

        // Build results output
        for (int vehicleIdx = 0; vehicleIdx < _schedulerContext.VehicleCount; vehicleIdx++)
        {
            var orders = new List<Order>();

            if (_schedulerContext.RoutingModel.IsVehicleUsed(solution, vehicleIdx))
            {
                var index = _schedulerContext.RoutingModel.Start(vehicleIdx);
                while (!_schedulerContext.RoutingModel.IsEnd(index))
                {
                    var nodeIndex = _schedulerContext.RoutingIndexManager.IndexToNode(index);
                    index = solution.Value(_schedulerContext.RoutingModel.NextVar(index));
                    if (nodeIndex != 0)
                    {
                        orders.Add(_schedulerContext.InputData.Orders[Convert.ToInt32(nodeIndex - 1)]);
                    }
                }
            }

            var keyValuePair = _schedulerContext.VehicleDriverAssignments[vehicleIdx];
            var schedule = new DriverSchedule
            {
                Orders = orders.ToArray(),
                Driver = keyValuePair.Value,
                Vehicle = keyValuePair.Key,
                EndLocation = _schedulerContext.InputData.Office.Location,
                StartLocation = _schedulerContext.InputData.Office.Location,
            };
            schedules.Add(schedule);
        }

        return new Schedule
        {
            Successful = true,
            DriverSchedules = schedules.ToArray(),
        };
    }

    private void PreferToUseAllVehicles()
    {
        long fixedVehicleCost = 10000;
        for (int vehicleIdx = 0; vehicleIdx < _schedulerContext.VehicleCount; vehicleIdx++)
        {
            _schedulerContext.RoutingModel.SetFixedCostOfVehicle(fixedVehicleCost, vehicleIdx);
        }
    }

    /// <summary>
    /// Adds a time dimension to the routing model. This tracks cumulative travel time and enforces an 8-hour limit.
    /// </summary>
    private void AddTimeDimension()
    {
        // Register a callback to convert distance (km) to time (minutes) assuming 40 km/h average speed
        int transitCallbackIndex = _schedulerContext.RoutingModel.RegisterTransitCallback((fromIndex, toIndex) =>
        {
            int fromNode = _schedulerContext.RoutingIndexManager.IndexToNode(fromIndex);
            int toNode = _schedulerContext.RoutingIndexManager.IndexToNode(toIndex);


            if (fromNode < 0 || fromNode >= _schedulerContext.LocationCount ||
                toNode < 0 || toNode >= _schedulerContext.LocationCount)
            {
                return 0;
            }

            double distanceKm = _schedulerContext.Distances[fromNode, toNode];
            double minutesRequired = distanceKm * (60.0 / Settings.DrivingSpeedKmPerHour);

            if (toNode != 0)
            {
                var order = _schedulerContext.InputData.Orders[toNode - 1];
                var products = _schedulerContext.InputData.Products
                    .Where(o => order.ProductIds.Contains(o.Id))
                    .ToArray();
                var setupMinutes = Utilities.EstimateSetupTime(products.ToArray());
                minutesRequired += Convert.ToDouble(setupMinutes);
                minutesRequired += Settings.BreakTimeBetweenAppointments;
            }

            return Convert.ToInt32(minutesRequired);
        });

        _schedulerContext.RoutingModel.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        // Add a time dimension with a max route duration of 480 minutes (8 hours)
        _schedulerContext.RoutingModel.AddDimension(
            transitCallbackIndex,
            0,      // no slack (waiting time)
            Settings.MinutesPerWorkday,    // max total time per route
            true,   // force all routes to start at time zero
            "Time");

        var timeDimension = _schedulerContext.RoutingModel.GetMutableDimension("Time");

        // Require each route to perform at least 1 minute of work
        for (int vehicleIdx = 0; vehicleIdx < _schedulerContext.VehicleCount; vehicleIdx++)
        {
            var endIndex = _schedulerContext.RoutingModel.End(vehicleIdx);
            timeDimension.CumulVar(endIndex).SetMin(1);
        }

        // 🔽 Add patient availability constraints based on order time windows
        for (int orderIdx = 0; orderIdx < _schedulerContext.InputData.Orders.Count; orderIdx++)
        {
            var order = _schedulerContext.InputData.Orders[orderIdx];
            int nodeIndex = orderIdx + 1; // +1 because node 0 is the depot
            var routingIndex = _schedulerContext.RoutingIndexManager.NodeToIndex(nodeIndex);

            int earliest, latest;
            if (order.AvailableTimes.Contains(TimeWindow.Morning) &&
                !order.AvailableTimes.Contains(TimeWindow.Afternoon))
            {
                earliest = Settings.MorningStartMinute;
                latest = Settings.MorningEndMinute;
            }
            else if (order.AvailableTimes.Contains(TimeWindow.Afternoon) &&
                     !order.AvailableTimes.Contains(TimeWindow.Morning))
            {
                earliest = Settings.AfternoonStartMinute;
                latest = Settings.AfternoonEndMinute;
            }
            else
            {
                earliest = Settings.MorningStartMinute;
                latest = Settings.AfternoonEndMinute;
            }

            timeDimension.CumulVar(routingIndex).SetRange(earliest, latest);
        }
    }

    /// <summary>
    /// Registers a callback function that returns the distance between two locations
    /// </summary>
    private void RegisterDistanceCallback()
    {
        // Create the distance callback
        // This callback returns the distance between two locations
        long DistanceCallback(long fromIndex, long toIndex)
        {
            // Convert from routing variable index to distance matrix index
            var fromNode = _schedulerContext.RoutingIndexManager.IndexToNode(fromIndex);
            var toNode = _schedulerContext.RoutingIndexManager.IndexToNode(toIndex);
            var distance = _schedulerContext.Distances[fromNode, toNode];
            return Convert.ToInt64(distance);
        }

        // Register the distance callback
        var transitCallbackIndex = _schedulerContext.RoutingModel.RegisterTransitCallback(DistanceCallback);
        _schedulerContext.RoutingModel.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);
    }
}