using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Models.Dtos;
using Synapse.DeliveryRoutes.Application.ViewModels;
using System.Text;

namespace Synapse.DeliveryRoutes.Application.Services;

public static class Utilities
{
    /// <summary>
    /// Builds the distance matrix using the Haversine formula (Level 1 approach)
    /// </summary>
    /// <param name="locations">Array of location coordinates where index 0 is the office/depot</param>
    /// <returns>The populated distance matrix</returns>
    public static double[,] BuildDistanceMatrix(GeoCoordinates[] locations)
    {
        var numberOfLocations = locations.Length;
        var matrix = new double[locations.Length, locations.Length];

        for (int i = 0; i < numberOfLocations; i++)
        {
            for (int j = 0; j < numberOfLocations; j++)
            {
                if (i == j)
                {
                    matrix[i, j] = 0; // Distance to self is zero
                    continue;
                }

                // Calculate Haversine distance between coordinates
                double distance = Utilities.CalculateHaversineDistance(
                    locations[i].Latitude,
                    locations[i].Longitude,
                    locations[j].Latitude,
                    locations[j].Longitude);

                // Convert to integer by scaling
                matrix[i, j] = distance * Settings.ScaleFactor;
            }
        }

        return matrix;
    }

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
                if (times.Count >= Settings.MinimumHistoricalSetupTimesToApplyStandardDeviation)
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
                    double stdDevBuffer = Math.Sqrt(filteredTimes.Average(v => Math.Pow(v - average, 2))) * Settings.HistoricalSetupTimesStandardDeviationTunableMultiple;
                    totalSetupTime += Convert.ToInt32(Math.Round(average + stdDevBuffer));
                }
            }
        }

        return Math.Max(Settings.MinimumDeliveryTimeMinutes, totalSetupTime);
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

            double minutesRequired = distanceKm * (60.0 / Settings.DrivingSpeedKmPerHour);
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

                foreach (var product in productsInOrder)
                {
                    output.AppendLine($"            - {product.Name}");
                }
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
    
    public static List<OrderAssignmentDto> BuildOrderAssignmentDtos(SchedulingInputData inputData)
    {
        var productById = inputData.Products.ToDictionary(p => p.Id);

        var orderDtos = inputData.Orders.Select(order =>
        {
            var requiredCerts = order.ProductIds
                .Select(pid => productById[pid].DeliveryRequirements.Certification)
                .Distinct()
                .ToHashSet();

            var acceptableVehicleTypes = order.ProductIds
                .Select(pid => productById[pid].DeliveryRequirements.TransportRequirements.VehicleTypes)
                .Aggregate((a, b) => a.Intersect(b).ToArray())
                .ToHashSet();

            var certifiedDrivers = inputData.Drivers
                .Where(d => requiredCerts.All(c => d.Certifications.Contains(c)))
                .ToList();

            var compatibleVehicles = inputData.Vehicles
                .Where(v => acceptableVehicleTypes.Contains(v.Type))
                .ToList();

            return new OrderAssignmentDto
            {
                Order = order,
                CertifiedDrivers = certifiedDrivers,
                CompatibleVehicles = compatibleVehicles
            };
        }).ToList();

        return orderDtos;
    }

    public static List<VehicleAssignmentDto> BuildVehicleAssignmentDtos(SchedulingInputData inputData)
    {
        var productById = inputData.Products.ToDictionary(p => p.Id);

        var vehicleDtos = inputData.Vehicles.Select(vehicle =>
        {
            var transportableOrders = inputData.Orders
                .Where(order =>
                {
                    var acceptableVehicleTypes = order.ProductIds
                        .Select(pid => productById[pid].DeliveryRequirements.TransportRequirements.VehicleTypes)
                        .Aggregate((a, b) => a.Intersect(b).ToArray())
                        .ToHashSet();

                    return acceptableVehicleTypes.Contains(vehicle.Type);
                })
                .ToList();

            var allowedDrivers = inputData.Drivers
                .Where(d => d.AllowedVehicles.Contains(vehicle.Type))
                .ToList();

            return new VehicleAssignmentDto
            {
                Vehicle = vehicle,
                TransportableOrders = transportableOrders,
                AllowedDrivers = allowedDrivers
            };
        }).ToList();

        return vehicleDtos;
    }

    public static List<DriverAssignmentDto> BuildDriverAssignmentDtos(SchedulingInputData inputData)
    {
        var productById = inputData.Products.ToDictionary(p => p.Id);

        var driverDtos = inputData.Drivers.Select(driver =>
        {
            var allowedVehicles = inputData.Vehicles
                .Where(v => driver.AllowedVehicles.Contains(v.Type))
                .ToList();

            var certifiedOrders = inputData.Orders
                .Where(order =>
                {
                    var requiredCerts = order.ProductIds
                        .Select(pid => productById[pid].DeliveryRequirements.Certification)
                        .Distinct()
                        .ToHashSet();

                    return requiredCerts.All(c => driver.Certifications.Contains(c));
                })
                .ToList();

            return new DriverAssignmentDto
            {
                Driver = driver,
                AllowedVehicles = allowedVehicles,
                CertifiedForOrders = certifiedOrders
            };
        }).ToList();

        return driverDtos;
    }

    public static (string Start, string End) GetDeliveryWindowRange(TimeWindow[] timeWindows)
    {
        bool hasMorning = timeWindows.Contains(TimeWindow.Morning);
        bool hasAfternoon = timeWindows.Contains(TimeWindow.Afternoon);

        if (hasMorning && hasAfternoon)
        {
            return ("09:00", "17:00"); // full day
        }
        else if (hasMorning)
        {
            return ("09:00", "13:00");
        }
        else if (hasAfternoon)
        {
            return ("13:00", "17:00");
        }

        // If no window provided, fallback to full day
        return ("09:00", "17:00");
    }

    public static DeliveryRouteViewModel[] ConvertToDeliveryRoutes(Schedule schedule, SchedulingInputData inputData)
    {
        if (!schedule.Successful || schedule.DriverSchedules == null)
            return Array.Empty<DeliveryRouteViewModel>();

        var productById = inputData.Products.ToDictionary(p => p.Id);

        var routes = new List<DeliveryRouteViewModel>();

        foreach (var driverSchedule in schedule.DriverSchedules)
        {
            var driver = driverSchedule.Driver;
            var vehicle = driverSchedule.Vehicle;
            var orders = driverSchedule.Orders;

            double totalDistanceMiles = 0;
            var deliveries = new List<DeliveryViewModel>();

            var locations = new List<GeoCoordinates> { driverSchedule.StartLocation }
                .Concat(orders.Select(o => o.Location))
                .Concat(new List<GeoCoordinates> { driverSchedule.EndLocation })
                .ToList();

            var currentTime = DateTime.Today.Add(Settings.MorningStartTime);

            for (int i = 0; i < orders.Length; i++)
            {
                var order = orders[i];
                var fromLocation = locations[i];
                var toLocation = locations[i + 1];

                double kilometersToDrive = Utilities.CalculateHaversineDistance(
                    fromLocation.Latitude, fromLocation.Longitude,
                    toLocation.Latitude, toLocation.Longitude);
                var distanceMiles = kilometersToDrive * 0.621371; // Convert km to miles
                totalDistanceMiles += distanceMiles;
                int driveTimeMinutes = Convert.ToInt32(kilometersToDrive * (60.0 / Settings.DrivingSpeedKmPerHour));

                // Lookup products for this order
                var products = order.ProductIds
                    .Select(pid => productById[pid])
                    .ToArray();

                var productViewModels = products.Select(product =>
                {
                    int productSetupTime = Utilities.EstimateSetupTime([product]);
                    return new DeliveryProductViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        RequiresSetup = new Random().Next(0, 1) == 1,
                        Quantity = new Random().Next(1, 3),
                        ExpectedSetupTimeMinutes = productSetupTime,
                        Weight = $"{product.Dimensions.Width} lbs",
                        ImageUrl = product.PhotoFileName
                    };
                }).ToList();

                int orderSetupTime = Utilities.EstimateSetupTime(products);

                var window = Utilities.GetDeliveryWindowRange(order.AvailableTimes);
                var delivery = new DeliveryViewModel
                {
                    OrderId = order.Id,
                    PatientName = order.PatientName,
                    PatientPhone = order.PatientPhone,
                    Address = order.Address,
                    Location = order.Location,
                    DeliveryWindow = new DeliveryWindowViewModel
                    {
                        Start = window.Start,
                        End = window.End
                    },
                    ExpectedDriveTimeMinutes = driveTimeMinutes,
                    ExpectedSetupTimeMinutes = orderSetupTime,
                    ExpectedArrivalTime = currentTime.ToString("HH:mm"),
                    DriveTimeDescription = $"Drive {distanceMiles:F1} miles in approx {driveTimeMinutes:HH:mm}",
                    ExpectedFinishTime = currentTime.AddMinutes(driveTimeMinutes + orderSetupTime).ToString("HH:mm"),
                    Products = productViewModels,
                    Notes = order.Notes
                };

                deliveries.Add(delivery);

                currentTime = currentTime.AddMinutes(driveTimeMinutes + orderSetupTime);
            }

            var route = new DeliveryRouteViewModel
            {
                Driver = new DriverViewModel
                {
                    Id = driver.Id,
                    Name = driver.Name,
                    Email = $"{driver.Name.ToLower().Replace(" ", ".")}@synapsehealth.com",
                    PhotoFilename = driver.PhotoFileName
                },
                Vehicle = new VehicleViewModel
                {
                    Id = vehicle.Id,
                    Type = vehicle.Type.ToString(),
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Year = vehicle.Year
                },
                Office = new OfficeViewModel
                {
                    Name = inputData.Office.Name,
                    Address = inputData.Office.Address,
                    Location = inputData.Office.Location
                },
                Summary = new RouteSummaryViewModel
                {
                    DistanceMiles = Math.Round(totalDistanceMiles, 1),
                    EstimateTimeOfReturnToBase = currentTime.ToString("HH:mm"),
                    EfficiencyPercent = 93, // hardcoded is fine
                    StopsCompleted = 0,
                    TotalStops = deliveries.Count
                },
                Deliveries = deliveries
            };

            routes.Add(route);
        }

        return routes.ToArray();
    }
}