using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

/// <summary>
/// Contains all data needed for the routing problem
/// </summary>
public class GetRoutingProblemDataResponse
{
    // Original data
    public GeoCoordinate[] Locations { get; }
    public DistanceMatrix DistanceMatrix { get; }
    public int[,] Distances { get; }

    // Derived parameters
    public int LocationCount => Locations.Length;
    public int Depot { get; } = 0; // Default depot is at index 0
    public int VehicleCount { get; }

    public GetRoutingProblemDataResponse(
        GeoCoordinate[] locations,
        DistanceMatrix distanceMatrix,
        int[,] distances,
        int vehicleCount)
    {
        Locations = locations;
        DistanceMatrix = distanceMatrix;
        Distances = distances;
        VehicleCount = vehicleCount;
    }

    public string GetDebugInfo()
    {
        return $"Routing Problem Data:\n" +
               $"- Locations: {LocationCount}\n" +
               $"- Vehicles: {VehicleCount}\n" +
               $"- Depot Index: {Depot}\n" +
               $"Distance Matrix:\n{DistanceMatrix.DebugString()}";
    }
}