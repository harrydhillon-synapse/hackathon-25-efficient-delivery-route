using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class DistanceMatrix
{
    private readonly double[,] _matrix;
    private readonly int _numberOfLocations;

    // Scale factor to convert floating point distances to integers
    //private const int SCALE_FACTOR = 1000;

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
    public double[,] Build(GeoCoordinate[] locations)
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
                double distance = CalculateHaversineDistance(
                    locations[i].Latitude,
                    locations[i].Longitude,
                    locations[j].Latitude,
                    locations[j].Longitude);

                // Convert to integer by scaling
                _matrix[i, j] = distance * ScheduleSolverSettings.ScaleFactor;
            }
        }

        return _matrix;
    }

    /// <summary>
    /// Calculates the "as the crow flies" distance between two points using the Haversine formula
    /// </summary>
    /// <returns>Distance in kilometers</returns>
    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Earth's radius in kilometers
        const double R = 6371.0;

        // Convert degrees to radians
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        // Haversine formula
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance in kilometers
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Gets the distance between two locations from the matrix
    /// </summary>
    public double GetDistance(int fromIndex, int toIndex)
    {
        return _matrix[fromIndex, toIndex];
    }

    /// <summary>
    /// Formats the distance matrix as a string for debugging
    /// </summary>
    public string DebugString()
    {
        var result = new System.Text.StringBuilder();
        result.AppendLine("Distance Matrix:");

        for (int i = 0; i < _numberOfLocations; i++)
        {
            for (int j = 0; j < _numberOfLocations; j++)
            {
                result.Append($"{_matrix[i, j],8}");
            }
            result.AppendLine();
        }

        return result.ToString();
    }
}