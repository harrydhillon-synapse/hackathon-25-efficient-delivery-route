using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Tests;

public static class TestData
{
    public static Product Product(string id, CertificationType cert, VehicleType[] vehicleTypes) => new()
    {
        Id = id,
        Name = id,
        Dimensions = new Dimensions(),
        DeliveryRequirements = new DeliveryRequirements
        {
            Certification = cert,
            TransportRequirements = new TransportRequirements
            {
                VehicleTypes = vehicleTypes,
                Orientation = Orientation.AnyPosition,
                StackingLimit = StackingLimit.FiveUnits,
                TemperatureControlled = false
            },
            PackagingType = PackagingType.OriginalManufacturerBox,
            SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
            SpecialHandling = "None"
        },
        PhotoFileName = ""
    };

    public static Order Order(string id, List<string> productIds) => new()
    {
        Id = id,
        ProductIds = productIds,
        Address = "",
        DeliveryDeadline = DateTime.Today,
        Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
        PatientName = "",
        PatientPhone = "",
        Priority = OrderPriority.Medium,
        AvailableTimes = []
    };

    public static Driver Driver(string id, List<CertificationType> certs, List<VehicleType> allowed) => new()
    {
        Id = id,
        Name = id,
        Certifications = certs,
        AllowedVehicles = allowed,
        Schedule = [],
        PhotoFileName = ""
    };

    public static Vehicle Vehicle(string id, VehicleType type) => new()
    {
        Id = id,
        Type = type,
        Make = "Test",
        Model = "Model",
        Year = 2020,
        Capacity = new VehicleCapacity(),
        Features = []
    };

    public static SchedulingInputData Input(
        List<Order> orders,
        List<Product> products,
        List<Driver> drivers,
        List<Vehicle> vehicles) => new()
    {
        Orders = orders,
        Products = products,
        Drivers = drivers,
        Vehicles = vehicles,
        Office = new Office
        {
            Id = "",
            Name = "",
            Address = "",
            Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
            Contact = new OfficeContactInfo
            {
                Email = "",
                Hours = "",
                Phone = ""
            }
        }
    };
}