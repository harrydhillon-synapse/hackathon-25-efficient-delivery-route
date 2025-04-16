using Synapse.DeliveryRoutes.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.DeliveryRoutes.Application.Services;

public class ScheduleSolver(SchedulingInputDataRepository _repository)
{
    public Result SolveSchedule()
    {
        // 1) Load all your domain data
        var inputData = _repository.LoadAllData();

        // 2) Create an array of all locations (office + orders)
        var locationCount = inputData.Orders.Count + 1; // +1 for the office
        var locations = new GeoCoordinate[locationCount];

        // 3) Add office as the first location (index 0)
        locations[0] = inputData.Office.OfficeGeocoordinates;

        // 4) Retrieve each Order's coordinates
        for (int i = 0; i < inputData.Orders.Count; i++)
        {
            // Add order location to the locations array (index i+1)
            locations[i + 1] = inputData.Orders[i].Coordinates;
        }

        // 5) Create and build the distance matrix
        var distanceMatrix = new DistanceMatrix(locationCount);
        var distances = distanceMatrix.Build(locations);

        // Debug output
        var matrixDebug = distanceMatrix.DebugString();

        return new Result
        {
            DebugOutput = $"Distance matrix created successfully.\n{matrixDebug}"
        };
    }
}