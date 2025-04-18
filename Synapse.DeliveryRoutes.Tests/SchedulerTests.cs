using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Services;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Synapse.DeliveryRoutes.Tests;

public class SchedulerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SchedulerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [ClassData(typeof(VehicleAssignmentTheoryData))]
    public void Constructor_Scheduler_Assigns_Vehicles_To_Drivers_AsExpected(
        string caseName,
        SchedulingInputData inputData,
        List<KeyValuePair<Vehicle, Driver>> expectedAssignments)
    {
        _testOutputHelper.WriteLine($"Running test case: {caseName}");

        // Act
        var scheduler = new Scheduler(inputData);
        var actualAssignments = scheduler.SchedulerContext.VehicleDriverAssignments;

        // Assert
        Assert.Equal(expectedAssignments.Count, actualAssignments.Count);

        foreach (var expected in expectedAssignments)
        {
            Assert.Contains(actualAssignments, a =>
                a.Key.Id == expected.Key.Id &&
                a.Value.Id == expected.Value.Id);
        }
    }

    public class VehicleAssignmentTheoryData : TheoryData<string, SchedulingInputData, List<KeyValuePair<Vehicle, Driver>>>
    {
        public VehicleAssignmentTheoryData()
        {
            // Case 1: Basic Match
            {
                var vehicle = new Vehicle
                {
                    Id = "V1",
                    Type = VehicleType.Truck,
                    Make = "Test",
                    Model = "ModelX",
                    Year = 2020,
                    Capacity = new VehicleCapacity()
                };

                var driver = new Driver
                {
                    Id = "D1",
                    Name = "Driver One",
                    Certifications = [CertificationType.HospitalBeds],
                    AllowedVehicles = [VehicleType.Truck]
                };

                var product = new Product
                {
                    Id = "P1",
                    Name = "Hospital Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = new[]
                            {
                                VehicleType.Truck
                            },
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "John Doe",
                    PatientPhone = "555-1234",
                    Address = "123 Main St",
                    Location = new GeoCoordinates
                    {
                        Latitude = 0,
                        Longitude = 0
                    },
                    AvailableTimes = Array.Empty<TimeWindow>(),
                    ProductIds = new List<string> { "P1" },
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office
                    {
                        Location = new GeoCoordinates
                        {
                            Latitude = 0,
                            Longitude = 0
                        },
                        Address = "",
                        Contact = new OfficeContactInfo
                        {
                            Email = "",
                            Hours = "",
                            Phone = ""
                        },
                        Id = "", 
                        Name = ""
                    },
                    Drivers = new List<Driver> { driver },
                    Vehicles = new List<Vehicle> { vehicle },
                    Products = new List<Product> { product },
                    Orders = new List<Order> { order }
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>
                {
                    new(vehicle, driver)
                };

                Add("Case 1 - Basic Match", input, expected);
            }

            // Case 2: No Driver Certifications Match
            {
                var vehicle = new Vehicle
                {
                    Id = "V2",
                    Type = VehicleType.Truck,
                    Make = "Test",
                    Model = "ModelY",
                    Year = 2021,
                    Capacity = new VehicleCapacity()
                };

                var driver = new Driver
                {
                    Id = "D2",
                    Name = "Driver Two",
                    Certifications = new List<CertificationType> { CertificationType.Basic },
                    AllowedVehicles = new List<VehicleType> { VehicleType.Truck }
                };

                var product = new Product
                {
                    Id = "P2",
                    Name = "Hospital Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = new[]
                            {
                                VehicleType.Truck
                            },
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O2",
                    PatientName = "Jane Doe",
                    PatientPhone = "555-5678",
                    Address = "456 Elm St",
                    Location = new GeoCoordinates
                    {
                        Latitude = 0,
                        Longitude = 0
                    },
                    AvailableTimes = Array.Empty<TimeWindow>(),
                    ProductIds = new List<string> { "P2" },
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office
                    {
                        Location = new GeoCoordinates
                        {
                            Latitude = 0,
                            Longitude = 0
                        },
                        Address = "",
                        Contact = new OfficeContactInfo
                        {
                            Email = "",
                            Hours = "",
                            Phone = ""
                        },
                        Id = "",
                        Name = ""
                    },
                    Drivers = new List<Driver> { driver },
                    Vehicles = new List<Vehicle> { vehicle },
                    Products = new List<Product> { product },
                    Orders = new List<Order> { order }
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>(); // No assignment

                Add("Case 2 - No Certification Match", input, expected);
            }

            // Case 3: Vehicle Type Mismatch
            {
                var vehicle = new Vehicle
                {
                    Id = "V3",
                    Type = VehicleType.Car, // Not acceptable for this product
                    Make = "Test",
                    Model = "ModelZ",
                    Year = 2022,
                    Capacity = new VehicleCapacity()
                };

                var driver = new Driver
                {
                    Id = "D3",
                    Name = "Driver Three",
                    Certifications = new List<CertificationType> { CertificationType.HospitalBeds },
                    AllowedVehicles = new List<VehicleType> { VehicleType.Car }
                };

                var product = new Product
                {
                    Id = "P3",
                    Name = "Hospital Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = new[]
                            {
                                VehicleType.Truck
                            },
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O3",
                    PatientName = "Sam Smith",
                    PatientPhone = "555-9999",
                    Address = "789 Oak St",
                    Location = new GeoCoordinates
                    {
                        Latitude = 0,
                        Longitude = 0
                    },
                    AvailableTimes = Array.Empty<TimeWindow>(),
                    ProductIds = new List<string> { "P3" },
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office
                    {
                        Location = new GeoCoordinates
                        {
                            Latitude = 0,
                            Longitude = 0
                        },
                        Address = "",
                        Contact = new OfficeContactInfo
                        {
                            Email = "",
                            Hours = "",
                            Phone = ""
                        },
                        Id = "",
                        Name = ""
                    },
                    Drivers = new List<Driver> { driver },
                    Vehicles = new List<Vehicle> { vehicle },
                    Products = new List<Product> { product },
                    Orders = new List<Order> { order }
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>(); // No assignment

                Add("Case 3 - Vehicle Type Mismatch", input, expected);
            }

            // Case 4: Driver has matching cert but not allowed to drive required vehicle
            {
                var vehicle = new Vehicle
                {
                    Id = "V1",
                    Type = VehicleType.Truck,
                    Make = "Test",
                    Model = "ModelX",
                    Year = 2020,
                    Capacity = new VehicleCapacity()
                };

                var driver = new Driver
                {
                    Id = "D1",
                    Name = "Driver One",
                    Certifications = [CertificationType.HospitalBeds],
                    AllowedVehicles = [VehicleType.Car] // Not allowed to drive Truck
                };

                var product = new Product
                {
                    Id = "P1",
                    Name = "Hospital Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Truck],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "Test",
                    PatientPhone = "555-1234",
                    Address = "123 Main St",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office
                    {
                        Id = "",
                        Name = "",
                        Address = "",
                        Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                        Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" }
                    },
                    Drivers = [driver],
                    Vehicles = [vehicle],
                    Products = [product],
                    Orders = [order]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>(); // No assignment
                Add("Case 4 - Driver not allowed to drive matching vehicle", input, expected);
            }

            // Case 5: Multiple drivers and vehicles, greedy assignment
            {
                var vehicle1 = new Vehicle { Id = "V1", Type = VehicleType.Truck, Make = "A", Model = "A", Year = 2020, Capacity = new VehicleCapacity() };
                var vehicle2 = new Vehicle { Id = "V2", Type = VehicleType.Truck, Make = "B", Model = "B", Year = 2021, Capacity = new VehicleCapacity() };

                var driver1 = new Driver { Id = "D1", Name = "Driver1", Certifications = [CertificationType.HospitalBeds], AllowedVehicles = [VehicleType.Truck] };
                var driver2 = new Driver { Id = "D2", Name = "Driver2", Certifications = [CertificationType.HospitalBeds], AllowedVehicles = [VehicleType.Truck] };

                var product = new Product
                {
                    Id = "P1",
                    Name = "Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Truck],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "A",
                    PatientPhone = "X",
                    Address = "A",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office { Id = "", Name = "", Address = "", Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                        Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" }
                    },
                    Drivers = [driver1, driver2],
                    Vehicles = [vehicle1, vehicle2],
                    Products = [product],
                    Orders = [order]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>
                {
                    new(vehicle1, driver1),
                    new(vehicle2, driver2)
                };

                Add("Case 5 - Greedy assignment logic", input, expected);
            }

            // Case 6: Driver has multiple certs, should only be assigned once
            {
                var vehicle = new Vehicle { Id = "V1", Type = VehicleType.Truck, Make = "A", Model = "A", Year = 2020, Capacity = new VehicleCapacity() };

                var driver = new Driver
                {
                    Id = "D1",
                    Name = "Driver1",
                    Certifications = [CertificationType.Mobility, CertificationType.HospitalBeds],
                    AllowedVehicles = [VehicleType.Truck]
                };

                var product = new Product
                {
                    Id = "P1",
                    Name = "Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Truck],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "Test",
                    PatientPhone = "",
                    Address = "X",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office { Id = "", Name = "", Address = "", Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                        Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" }
                    },
                    Drivers = [driver],
                    Vehicles = [vehicle],
                    Products = [product],
                    Orders = [order]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>
                {
                    new(vehicle, driver)
                };

                Add("Case 6 - Driver with multiple certifications", input, expected);
            }

            // Case 7: No assignable orders (invalid cert & vehicle)
            {
                var vehicle = new Vehicle { Id = "V1", Type = VehicleType.Car, Make = "A", Model = "B", Year = 2020, Capacity = new VehicleCapacity() };
                var driver = new Driver
                {
                    Id = "D1",
                    Name = "Driver1",
                    Certifications = [CertificationType.Basic],
                    AllowedVehicles = [VehicleType.Car]
                };

                var product = new Product
                {
                    Id = "P1",
                    Name = "Complex Product",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.Complex,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Car],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "Patient",
                    PatientPhone = "555",
                    Address = "Somewhere",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office { Id = "", Name = "", Address = "", Location = new GeoCoordinates { Latitude = 0, Longitude = 0 }, Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" } },
                    Drivers = [driver],
                    Vehicles = [vehicle],
                    Products = [product],
                    Orders = [order]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>();
                Add("Case 7 - No assignable orders", input, expected);
            }

            // Case 8: Multiple orders with full assignment
            {
                var vehicle1 = new Vehicle { Id = "V1", Type = VehicleType.Car, Make = "A", Model = "A", Year = 2020, Capacity = new VehicleCapacity() };
                var vehicle2 = new Vehicle { Id = "V2", Type = VehicleType.Truck, Make = "B", Model = "B", Year = 2021, Capacity = new VehicleCapacity() };

                var driver1 = new Driver { Id = "D1", Name = "D1", Certifications = [CertificationType.Basic], AllowedVehicles = [VehicleType.Car] };
                var driver2 = new Driver { Id = "D2", Name = "D2", Certifications = [CertificationType.HospitalBeds], AllowedVehicles = [VehicleType.Truck] };

                var product1 = new Product
                {
                    Id = "P1",
                    Name = "Nebulizer",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.Basic,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Car],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var product2 = new Product
                {
                    Id = "P2",
                    Name = "Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Truck],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order1 = new Order
                {
                    Id = "O1",
                    PatientName = "",
                    PatientPhone = "",
                    Address = "",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var order2 = new Order
                {
                    Id = "O2",
                    PatientName = "",
                    PatientPhone = "",
                    Address = "",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P2"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office { Id = "", Name = "", Address = "", Location = new GeoCoordinates { Latitude = 0, Longitude = 0 }, Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" } },
                    Drivers = [driver1, driver2],
                    Vehicles = [vehicle1, vehicle2],
                    Products = [product1, product2],
                    Orders = [order1, order2]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>
                {
                    new(vehicle1, driver1),
                    new(vehicle2, driver2)
                };

                Add("Case 8 - All valid assignments", input, expected);
            }

            // Case 9: Cert match but vehicle type mismatch
            {
                var vehicle = new Vehicle { Id = "V1", Type = VehicleType.Car, Make = "Z", Model = "Z", Year = 2022, Capacity = new VehicleCapacity() };
                var driver = new Driver
                {
                    Id = "D1",
                    Name = "Driver1",
                    Certifications = [CertificationType.Complex],
                    AllowedVehicles = [VehicleType.Car]
                };

                var product = new Product
                {
                    Id = "P1",
                    Name = "Ventilator",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.Complex,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Truck],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "",
                    PatientPhone = "",
                    Address = "",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office { Id = "", Name = "", Address = "", Location = new GeoCoordinates { Latitude = 0, Longitude = 0 }, Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" } },
                    Drivers = [driver],
                    Vehicles = [vehicle],
                    Products = [product],
                    Orders = [order]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>();
                Add("Case 9 - Certification match but wrong vehicle", input, expected);
            }

            // Case 10: Extra drivers and vehicles
            {
                var vehicle1 = new Vehicle { Id = "V1", Type = VehicleType.Truck, Make = "A", Model = "B", Year = 2020, Capacity = new VehicleCapacity() };
                var vehicle2 = new Vehicle { Id = "V2", Type = VehicleType.Truck, Make = "C", Model = "D", Year = 2021, Capacity = new VehicleCapacity() };

                var driver1 = new Driver { Id = "D1", Name = "D1", Certifications = [CertificationType.HospitalBeds], AllowedVehicles = [VehicleType.Truck] };
                var driver2 = new Driver { Id = "D2", Name = "D2", Certifications = [CertificationType.HospitalBeds], AllowedVehicles = [VehicleType.Truck] };

                var product = new Product
                {
                    Id = "P1",
                    Name = "Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Truck],
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "",
                    PatientPhone = "",
                    Address = "",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office { Id = "", Name = "", Address = "", Location = new GeoCoordinates { Latitude = 0, Longitude = 0 }, Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" } },
                    Drivers = [driver1, driver2],
                    Vehicles = [vehicle1, vehicle2],
                    Products = [product],
                    Orders = [order]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>
                {
                    new(vehicle1, driver1),
                    new(vehicle2, driver2)
                };

                Add("Case 10 - Extra vehicles and drivers", input, expected);
            }

            // Case 11: Single driver, car & truck eligible, must choose truck
            {
                var car = new Vehicle { Id = "Car1", Type = VehicleType.Car, Make = "A", Model = "B", Year = 2020, Capacity = new VehicleCapacity() };
                var truck = new Vehicle { Id = "Truck1", Type = VehicleType.Truck, Make = "X", Model = "Y", Year = 2021, Capacity = new VehicleCapacity() };

                var driver = new Driver
                {
                    Id = "D1",
                    Name = "Driver1",
                    Certifications = [CertificationType.HospitalBeds],
                    AllowedVehicles = [VehicleType.Car, VehicleType.Truck]
                };

                var product1 = new Product
                {
                    Id = "P1",
                    Name = "Hospital Bed",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Truck], // Must be truck
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var product2 = new Product
                {
                    Id = "P2",
                    Name = "CPAP",
                    Dimensions = new Dimensions(),
                    DeliveryRequirements = new DeliveryRequirements
                    {
                        Certification = CertificationType.HospitalBeds,
                        PackagingType = PackagingType.OriginalManufacturerBox,
                        SetupAssistance = SetupAssistanceLevel.Level1BasicAssemblyRequired,
                        TransportRequirements = new TransportRequirements
                        {
                            VehicleTypes = [VehicleType.Car, VehicleType.Truck], // Must be truck
                            Orientation = Orientation.AnyPosition,
                            StackingLimit = StackingLimit.FiveUnits,
                            TemperatureControlled = false
                        },
                        SpecialHandling = "None"
                    }
                };

                var order = new Order
                {
                    Id = "O1",
                    PatientName = "Test",
                    PatientPhone = "123",
                    Address = "123 Main St",
                    Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                    AvailableTimes = [],
                    ProductIds = ["P1", "P2"],
                    DeliveryDeadline = DateOnly.FromDateTime(DateTime.Today),
                    Priority = OrderPriority.Medium
                };

                var input = new SchedulingInputData
                {
                    Office = new Office
                    {
                        Id = "",
                        Name = "",
                        Address = "",
                        Location = new GeoCoordinates { Latitude = 0, Longitude = 0 },
                        Contact = new OfficeContactInfo { Phone = "", Email = "", Hours = "" }
                    },
                    Drivers = [driver],
                    Vehicles = [car, truck],
                    Products = [product1, product2],
                    Orders = [order]
                };

                var expected = new List<KeyValuePair<Vehicle, Driver>>
                {
                    new(truck, driver)
                };

                Add("Case 11 - Driver must use truck, not car", input, expected);
            }
        }
    }

    [Theory]
    [InlineData(DataSet.Original)]
    [InlineData(DataSet.Test)]
    [InlineData(DataSet.DemoSimple)]
    [InlineData(DataSet.DemoComplex)]
    public void CreateSchedule(DataSet dataSet)
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData(dataSet);
        var result = new Scheduler(inputData).CreateSchedule();
        Assert.NotNull(result);
        Assert.True(result.Successful);

        _testOutputHelper.WriteLine(Utilities.ToString(result, inputData.Products.ToArray()));
    }
}