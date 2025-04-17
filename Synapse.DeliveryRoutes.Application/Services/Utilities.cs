using Synapse.DeliveryRoutes.Application.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Synapse.DeliveryRoutes.Application.Services;

public static class Utilities
{
    /// <summary>
    /// Calculates the "as the crow flies" distance between two points using the Haversine formula
    /// </summary>
    /// <returns>Distance in kilometers</returns>
    public static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
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

    public static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public static int EstimateSetupTime(Product[] products)
    {
        int totalSetupTime = 0;

        foreach (var product in products)
        {
            var times = product.DeliveryRequirements.HistoricalSetupTimes;

            if (times.Count > 0)
            {
                List<int> filteredTimes;
                if (times.Count >= ScheduleSolverSettings.MinimumHistoricalSetupTimesToApplyStandardDeviation)
                {
                    double mean = times.Average();
                    double stdDev = Math.Sqrt(times.Average(v => Math.Pow(v - mean, 2)));
                    filteredTimes = times.Where(t => Math.Abs(t - mean) <= 2 * stdDev).ToList();
                }
                else
                {
                    filteredTimes = times.ToList(); // no filtering
                }

                if (filteredTimes.Count > 0)
                {
                    double average = filteredTimes.Average();
                    double stdDevBuffer = Math.Sqrt(filteredTimes.Average(v => Math.Pow(v - average, 2))) * ScheduleSolverSettings.HistoricalSetupTimesStandardDeviationTunableMultiple;
                    totalSetupTime += Convert.ToInt32(Math.Round(average + stdDevBuffer));
                }
            }
        }

        return Math.Max(ScheduleSolverSettings.MinimumDeliveryTimeMinutes, totalSetupTime);
    }

    public static string ToString(Schedule schedule, Product[] allProducts)
    {
        if (!schedule.Successful)
        {
            return "Unable to create schedule";
        }

        var output = new StringBuilder();

        foreach (var driverSchedule in schedule.DriverSchedules!)
        {
            output.AppendLine(Utilities.ToString(driverSchedule, allProducts));
            output.AppendLine();
        }

        return output.ToString();
    }

    private static string ToString(DriverSchedule driverSchedule, Product[] allProducts)
    {
        if (driverSchedule.Orders?.Any() != true)
        {
            return $"{driverSchedule.Driver.Name} has no deliveries.";
        }

        var output = new StringBuilder();
        output.AppendLine($"{driverSchedule.Driver.Name} (Vehicle {driverSchedule.Vehicle.Id} - {driverSchedule.Vehicle.Type}) has {driverSchedule.Orders.Length} deliveries.");

        var locations = new List<GeoCoordinates> { driverSchedule.StartLocation }
            .Concat(driverSchedule.Orders.Select(o => o.Location))
            .Concat(new List<GeoCoordinates> { driverSchedule.EndLocation })
            .ToArray();

        var totalDrivingMinutes = 0;
        var totalSetupMinutes = 0;

        for (var i = 0; i < locations.Length - 1; i++)
        {
            var fromLocation = locations[i];
            var toLocation = locations[i + 1];

            double distanceKm = Utilities.CalculateHaversineDistance(
                fromLocation.Latitude, fromLocation.Longitude,
                toLocation.Latitude, toLocation.Longitude);

            double minutesRequired = distanceKm * (60.0 / ScheduleSolverSettings.DrivingSpeedKmPerHour);
            int drivingMinutes = Convert.ToInt32(minutesRequired);
            totalDrivingMinutes += drivingMinutes;

            if (i < driverSchedule.Orders.Length)
            {
                var order = driverSchedule.Orders[i];
                var productsInOrder = allProducts.Where(o => order.ProductIds.Contains(o.Id)).ToArray();
                int setupTime = Utilities.EstimateSetupTime(productsInOrder);
                totalSetupMinutes += setupTime;

                output.AppendLine($"    {i + 1}) Deliver order {order.Id} to patient {order.PatientName} at")
                      .AppendLine($"        {order.Address}")
                      .AppendLine($"        → Drive {distanceKm:F1} km in approx {drivingMinutes} min")
                      .AppendLine($"        → Setup {order.ProductIds.Count} items in approx {setupTime} min");
            }
            else
            {
                output.AppendLine($"    {i + 1}) Drive to end location");
                output.AppendLine($"        → Drive {distanceKm:F1} km in approx {drivingMinutes} min");
            }
        }

        output.AppendLine($"Total driving time: {TimeSpan.FromMinutes(totalDrivingMinutes):hh\\:mm}");
        output.AppendLine($"Total setup time: {TimeSpan.FromMinutes(totalSetupMinutes):hh\\:mm}");
        output.AppendLine($"Total time: {TimeSpan.FromMinutes(totalDrivingMinutes + totalSetupMinutes):hh\\:mm}");

        return output.ToString();
    }
}