using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;
using System.Text;

namespace Synapse.DeliveryRoutes.Application.Services;

public class Scheduler
{
    public Schedule CreateSchedule(SchedulerContext schedulerContext)
    {
        RegisterDistanceCallback(schedulerContext);
        AddTimeDimension(schedulerContext);

        long fixedVehicleCost = 10000;
        for (int vehicleIdx = 0; vehicleIdx < schedulerContext.VehicleCount; vehicleIdx++)
        {
            schedulerContext.RoutingModel.SetFixedCostOfVehicle(fixedVehicleCost, vehicleIdx);
        }

        // Solver parameters
        var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = ScheduleSolverSettings.TimeLimitInSeconds };

        // Solve
        var solution = schedulerContext.RoutingModel.SolveWithParameters(searchParameters);

        if (solution == null)
        {
            return new Schedule { Successful = false, DriverSchedules = null };
        }

        var schedules = new List<DriverSchedule>();

        // Build results output
        var output = new StringBuilder();
        output.AppendLine("Solution found. Vehicle routes:");

        for (int vehicleIdx = 0; vehicleIdx < schedulerContext.VehicleCount; vehicleIdx++)
        {
            var keyValuePair = schedulerContext.VehicleDriverAssignments[vehicleIdx];
            var vehicle = keyValuePair.Key;
            var driver = keyValuePair.Value;

            if (!schedulerContext.RoutingModel.IsVehicleUsed(solution, vehicleIdx))
            {
                output.AppendLine($"Driver {driver.Name} ({driver.DriverID}) assigned to vehicle {vehicle.VehicleID} ({vehicle.Type}) - Not scheduled for any deliveries this day");
                continue;
            }

            var locations = new List<object>();
            var index = schedulerContext.RoutingModel.Start(vehicleIdx);
            while (!schedulerContext.RoutingModel.IsEnd(index))
            {
                var nodeIndex = schedulerContext.RoutingIndexManager.IndexToNode(index);
                //locations.Add($" -> {nodeIndex}");
                index = solution.Value(schedulerContext.RoutingModel.NextVar(index));
                if (nodeIndex == 0)
                {
                    locations.Add("Depot");
                }
                else
                {
                    locations.Add(schedulerContext.InputData.Orders[Convert.ToInt32(nodeIndex - 1)]);
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

        return new Schedule
        {
            Successful = true,
            DriverSchedules = schedules.ToArray(),
        };
    }

    /// <summary>
    /// Adds a time dimension to the routing model. This tracks cumulative travel time and enforces an 8-hour limit.
    /// </summary>
    private void AddTimeDimension(SchedulerContext schedulerContext)
    {
        // Register a callback to convert distance (km) to time (minutes) assuming 40 km/h average speed
        int transitCallbackIndex = schedulerContext.RoutingModel.RegisterTransitCallback((fromIndex, toIndex) =>
        {
            var fromNode = schedulerContext.RoutingIndexManager.IndexToNode(fromIndex);
            var toNode = schedulerContext.RoutingIndexManager.IndexToNode(toIndex);
            double distanceKm = schedulerContext.Distances[fromNode, toNode];
            double timeMinutes = distanceKm * 1.5; // 40 km/h → 1.5 minutes per km
            return Convert.ToInt32(timeMinutes);
        });

        // Add a time dimension with a max route duration of 480 minutes (8 hours)
        schedulerContext.RoutingModel.AddDimension(
            transitCallbackIndex,
            0,      // no slack (waiting time)
            ScheduleSolverSettings.MinutesPerWorkday,    // max total time per route
            true,   // force all routes to start at time zero
            "Time");

        var timeDimension = schedulerContext.RoutingModel.GetMutableDimension("Time");

        // Require each route to perform at least 1 minute of work
        for (int vehicleIdx = 0; vehicleIdx < schedulerContext.VehicleCount; vehicleIdx++)
        {
            var endIndex = schedulerContext.RoutingModel.End(vehicleIdx);
            timeDimension.CumulVar(endIndex).SetMin(1);
        }
    }

    /// <summary>
    /// Registers a callback function that returns the distance between two locations
    /// </summary>
    private void RegisterDistanceCallback(SchedulerContext schedulerContext)
    {
        // Create the distance callback
        // This callback returns the distance between two locations
        long DistanceCallback(long fromIndex, long toIndex)
        {
            // Convert from routing variable index to distance matrix index
            var fromNode = schedulerContext.RoutingIndexManager.IndexToNode(fromIndex);
            var toNode = schedulerContext.RoutingIndexManager.IndexToNode(toIndex);
            return Convert.ToInt64(schedulerContext.Distances[fromNode, toNode]);
        }

        // Register the distance callback
        var transitCallbackIndex = schedulerContext.RoutingModel.RegisterTransitCallback(DistanceCallback);
        schedulerContext.RoutingModel.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);
    }
}