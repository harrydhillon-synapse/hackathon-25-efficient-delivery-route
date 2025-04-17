﻿using Google.OrTools.ConstraintSolver;
using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class Scheduler(SchedulerContext schedulerContext)
{
    public Schedule CreateSchedule()
    {
        RegisterDistanceCallback();
        AddTimeDimension();
        PreferToUseAllVehicles();

        // Solver parameters
        var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;
        searchParameters.LocalSearchMetaheuristic = LocalSearchMetaheuristic.Types.Value.GuidedLocalSearch;
        searchParameters.TimeLimit = new Google.Protobuf.WellKnownTypes.Duration { Seconds = ScheduleSolverSettings.TimeLimitInSeconds };

        // Solve
        var solution = schedulerContext.RoutingModel.SolveWithParameters(searchParameters);

        return ProcessSolution(solution);
    }

    private Schedule ProcessSolution(Assignment? solution)
    {
        if (solution == null)
        {
            return new Schedule { Successful = false, DriverSchedules = null };
        }

        var schedules = new List<DriverSchedule>();

        // Build results output
        for (int vehicleIdx = 0; vehicleIdx < schedulerContext.VehicleCount; vehicleIdx++)
        {
            var orders = new List<Order>();

            if (schedulerContext.RoutingModel.IsVehicleUsed(solution, vehicleIdx))
            {
                var index = schedulerContext.RoutingModel.Start(vehicleIdx);
                while (!schedulerContext.RoutingModel.IsEnd(index))
                {
                    var nodeIndex = schedulerContext.RoutingIndexManager.IndexToNode(index);
                    index = solution.Value(schedulerContext.RoutingModel.NextVar(index));
                    if (nodeIndex != 0)
                    {
                        orders.Add(schedulerContext.InputData.Orders[Convert.ToInt32(nodeIndex - 1)]);
                    }
                }
            }

            var keyValuePair = schedulerContext.VehicleDriverAssignments[vehicleIdx];
            var schedule = new DriverSchedule
            {
                Orders = orders.ToArray(),
                Driver = keyValuePair.Value,
                Vehicle = keyValuePair.Key,
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
        for (int vehicleIdx = 0; vehicleIdx < schedulerContext.VehicleCount; vehicleIdx++)
        {
            schedulerContext.RoutingModel.SetFixedCostOfVehicle(fixedVehicleCost, vehicleIdx);
        }
    }

    /// <summary>
    /// Adds a time dimension to the routing model. This tracks cumulative travel time and enforces an 8-hour limit.
    /// </summary>
    private void AddTimeDimension()
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
    private void RegisterDistanceCallback()
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