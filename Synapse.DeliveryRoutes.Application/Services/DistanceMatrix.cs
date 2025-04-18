using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class DistanceMatrix
{
    private readonly double[,] _matrix;
    private readonly int _numberOfLocations;
    
    public DistanceMatrix(int numberOfLocations)
    {
        _numberOfLocations = numberOfLocations;
        _matrix = new double[numberOfLocations, numberOfLocations];
    }

    /// <summary>
    /// Builds the distance matrix using the Haversine formula (Level 1 approach)
    /// </summary>
    /// <param name="locations">Array of location coordinates where index 0 is the office/depot</param>
    /// <returns>The populated distance matrix</returns>
    public double[,] Build(GeoCoordinates[] locations)
    {
        for (int i = 0; i < _numberOfLocations; i++)
        {
            for (int j = 0; j < _numberOfLocations; j++)
            {
                if (i == j)
                {
                    _matrix[i, j] = 0; // Distance to self is zero
                    continue;
                }

                // Calculate Haversine distance between coordinates
                double distance = Utilities.CalculateHaversineDistance(
                    locations[i].Latitude,
                    locations[i].Longitude,
                    locations[j].Latitude,
                    locations[j].Longitude);

                // Convert to integer by scaling
                _matrix[i, j] = distance * Settings.ScaleFactor;
            }
        }

        return _matrix;
    }
}