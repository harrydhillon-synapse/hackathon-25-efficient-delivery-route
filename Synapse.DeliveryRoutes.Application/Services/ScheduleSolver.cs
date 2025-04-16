using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;
using System.Text;

namespace Synapse.DeliveryRoutes.Application.Services;

public class ScheduleSolver
{
    public Result SolveSchedule(SchedulingInputData inputData)
    {
        var routingProblemDataRequest = new GetRoutingDataRequest
        {
            OfficeCoordinate = inputData.Office.OfficeGeocoordinates,
            DeliveryCoordinates = inputData.Orders.Select(o => o.Geocoordinates).ToList(),
            VehicleCount = inputData.Vehicles.Count
        };

        // Get matrix
        var routingProblemDataResponse = GetRoutingProblemData(routingProblemDataRequest);


        // === DRIVER-VEHICLE MATCHING (1-to-1) ===

        // Create a list of vehicle-driver pairs where the driver can operate the vehicle
        var compatibleAssignments = (from driver in inputData.Drivers
            from vehicle in inputData.Vehicles
            where driver.VehiclesCanDrive.Contains(vehicle.Type)
            select new { driver, vehicle }).ToList();

        // Select a distinct 1-to-1 assignment (greedy)
        var assignedDrivers = new HashSet<string>();
        var assignedVehicles = new HashSet<string>();
        var finalAssignments = new List<(Driver driver, Vehicle vehicle)>();

        foreach (var pair in compatibleAssignments)
        {
            if (assignedDrivers.Contains(pair.driver.DriverID) || assignedVehicles.Contains(pair.vehicle.VehicleID))
                continue;

            finalAssignments.Add((pair.driver, pair.vehicle));
            assignedDrivers.Add(pair.driver.DriverID);
            assignedVehicles.Add(pair.vehicle.VehicleID);
        }

        // Now we only use the vehicles with matched drivers (1:1)
        var vehicleCount = finalAssignments.Count;
        var filteredVehicles = finalAssignments.Select(p => p.vehicle).ToList();
        var filteredDrivers = finalAssignments.Select(p => p.driver).ToList();













        // Create the routing index manager
        var manager = new RoutingIndexManager(
            routingProblemDataResponse.LocationCount,
            vehicleCount,
            routingProblemDataResponse.Depot);

        // Create the routing model
        var routingModel = new RoutingModel(manager);


        // 5) Register the distance callback function
        int transitCallbackIndex = RegisterDistanceCallback(routingModel, manager, routingProblemDataResponse);

        // 6) Set the cost of travel (arc cost evaluator)
        routingModel.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        // Vehicle Compatibility Constraint - does not work
        //long fixedVehicleCost = 10000;
        //for (int vehicleIdx = 0; vehicleIdx < vehicleCount; vehicleIdx++)
        //{
        //    var startIndex = routingModel.Start(vehicleIdx);
        //    //var endIndex = routingModel.End(vehicleIdx);

        //    // Add a penalty for skipping the start node (i.e., not using the vehicle at all)
        //    routingModel.AddDisjunction(new[] { startIndex }, penalty: long.MaxValue);
        //}

        // Solver parameters
        var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = ScheduleSolverSettings.TimeLimitInSeconds };

        // Solve
        var solution = routingModel.SolveWithParameters(searchParameters);

        if (solution == null)
        {
            return new Result { Successful = false, DebugOutput = "No solution found." };
        }
        // Build results output
        var output = new StringBuilder();
        output.AppendLine("Solution found. Vehicle routes:");
        // Assign drivers to vehicles (one driver per used vehicle)
        var availableDrivers = new List<Driver>(inputData.Drivers);

        for (int vehicleIdx = 0; vehicleIdx < routingProblemDataRequest.VehicleCount; vehicleIdx++)
        {
            if (!routingModel.IsVehicleUsed(solution, vehicleIdx)) continue;

            var vehicle = filteredVehicles[vehicleIdx];
            var assignedDriver = availableDrivers
                .FirstOrDefault(driver => driver.VehiclesCanDrive.Contains(vehicle.Type));

            if (assignedDriver == null)
            {
                output.AppendLine($"Vehicle {vehicleIdx} (Type: {vehicle.Type}) - No compatible driver available (unexpected)");
                continue;
            }

            output.AppendLine($"Vehicle {vehicleIdx} (Type: {vehicle.Type}) - Assigned to Driver {assignedDriver.DriverID} - {assignedDriver.Name}:");
            availableDrivers.Remove(assignedDriver);

            var index = routingModel.Start(vehicleIdx);
            while (!routingModel.IsEnd(index))
            {
                var nodeIndex = manager.IndexToNode(index);
                output.Append($"{nodeIndex} -> ");
                index = solution.Value(routingModel.NextVar(index));
            }
            output.AppendLine("End");
        }


        return new Result
        {
            Successful = true,
            DebugOutput = output.ToString()
        };
    }

    /// <summary>
    /// Registers a callback function that returns the distance between two locations
    /// </summary>
    private int RegisterDistanceCallback(RoutingModel routing, RoutingIndexManager manager, GetRoutingProblemDataResponse data)
    {
        // Create the distance callback
        // This callback returns the distance between two locations
        long DistanceCallback(long fromIndex, long toIndex)
        {
            // Convert from routing variable index to distance matrix index
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return data.Distances[fromNode, toNode];
        }

        // Register the distance callback
        return routing.RegisterTransitCallback(DistanceCallback);
    }

    private GetRoutingProblemDataResponse GetRoutingProblemData(GetRoutingDataRequest request)
    {
        // 2) Create an array of all locations (office + orders)
        var deliveryCoordinates = request.DeliveryCoordinates.ToList();
        var locationCount = deliveryCoordinates.Count + 1; // +1 for the office
        var locations = new GeoCoordinate[locationCount];

        // 3) Add office as the first location (index 0)
        locations[0] = request.OfficeCoordinate;

        // 4) Retrieve each Order's coordinates
        for (int i = 0; i < deliveryCoordinates.Count; i++)
        {
            // Add order location to the locations array (index i+1)
            locations[i + 1] = deliveryCoordinates[i];
        }

        // 5) Create and build the distance matrix
        var distanceMatrix = new DistanceMatrix(locationCount);
        var distances = distanceMatrix.Build(locations);

        return new GetRoutingProblemDataResponse(locations, distanceMatrix, distances, request.VehicleCount);
    }
}