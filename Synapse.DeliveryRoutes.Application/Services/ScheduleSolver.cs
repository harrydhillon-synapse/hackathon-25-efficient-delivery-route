using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;

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

        // Create the routing index manager
        var manager = new RoutingIndexManager(
            routingProblemDataResponse.LocationCount,
            routingProblemDataResponse.VehicleCount,
            routingProblemDataResponse.Depot);

        // Create the routing model
        var routingModel = new RoutingModel(manager);


        // 5) Register the distance callback function
        int transitCallbackIndex = RegisterDistanceCallback(routingModel, manager, routingProblemDataResponse);

        // 6) Set the cost of travel (arc cost evaluator)
        routingModel.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        // Vehicle Compatibility Constraint
        long fixedVehicleCost = 10000;
        for (int vehicleIdx = 0; vehicleIdx < inputData.Vehicles.Count; vehicleIdx++)
        {
            var vehicle = inputData.Vehicles[vehicleIdx];
            routingModel.SetFixedCostOfVehicle(fixedVehicleCost, vehicleIdx);

            // Allow vehicle only if there is a compatible driver
            var hasCompatibleDriver = inputData.Drivers
                .Any(driver => driver.VehiclesCanDrive.Contains(vehicle.Type));

            if (!hasCompatibleDriver)
            {
                // Set an extremely high fixed cost to strongly discourage solver from using it
                routingModel.SetFixedCostOfVehicle(long.MaxValue, vehicleIdx);
                routingModel.SetVehicleUsedWhenEmpty(false, vehicleIdx);
                // REMOVE the AddDisjunction line completely.
            }
            else
            {
                routingModel.SetFixedCostOfVehicle(fixedVehicleCost, vehicleIdx);
            }
        }

        // Solver parameters
        var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 30 };

        // Solve
        var solution = routingModel.SolveWithParameters(searchParameters);

        if (solution == null)
        {
            return new Result { DebugOutput = "No solution found." };
        }

        // Build results
        return new Result
        {
            DebugOutput = "Solution found. Vehicles used: " + Enumerable.Range(0, inputData.Vehicles.Count)
                .Count(vehicleIdx => routingModel.IsVehicleUsed(solution, vehicleIdx))
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