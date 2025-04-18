using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Models.Dtos;
using Synapse.DeliveryRoutes.Application.Services;
using Xunit.Abstractions;

namespace Synapse.DeliveryRoutes.Tests;

public class UtilitiesTests
{
    private readonly ITestOutputHelper _output;

    public UtilitiesTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [ClassData(typeof(BuildOrderAssignmentDtosTheoryData))]
    public void BuildOrderAssignmentDtos_ReturnsExpectedResults(
        string caseName,
        SchedulingInputData inputData,
        List<OrderAssignmentDto> expectedOrderDtos)
    {
        // Log case name for easy debugging
        _output.WriteLine($"Running test case: {caseName}");

        // Act
        var result = Utilities.BuildOrderAssignmentDtos(inputData);

        // Assert
        Assert.Equal(expectedOrderDtos.Count, result.Count);

        foreach (var expected in expectedOrderDtos)
        {
            var match = result.SingleOrDefault(r => r.Order.Id == expected.Order.Id);
            Assert.NotNull(match);

            Assert.Equal(
                expected.CertifiedDrivers.Select(d => d.Id).OrderBy(x => x),
                match.CertifiedDrivers.Select(d => d.Id).OrderBy(x => x));

            Assert.Equal(
                expected.CompatibleVehicles.Select(v => v.Id).OrderBy(x => x),
                match.CompatibleVehicles.Select(v => v.Id).OrderBy(x => x));
        }
    }

    public class BuildOrderAssignmentDtosTheoryData
        : TheoryData<string, SchedulingInputData, List<OrderAssignmentDto>>
    {
        public BuildOrderAssignmentDtosTheoryData()
        {
            // Case 1: Basic Match
            {
                var product = TestData.Product("P1", CertificationType.HospitalBeds, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);
                var driver = TestData.Driver("D1", [CertificationType.HospitalBeds], [VehicleType.Truck]);
                var vehicle = TestData.Vehicle("V1", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver], [vehicle]);

                Add("Case 1 - Basic Match", input, [
                    new OrderAssignmentDto
                {
                    Order = order,
                    CertifiedDrivers = [driver],
                    CompatibleVehicles = [vehicle]
                }
                ]);
            }

            // Case 2: No Certified Drivers
            {
                var product = TestData.Product("P1", CertificationType.Complex, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);
                var driver = TestData.Driver("D1", [CertificationType.Basic], [VehicleType.Truck]); // not qualified
                var vehicle = TestData.Vehicle("V1", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver], [vehicle]);

                Add("Case 2 - No Certified Drivers", input, [
                    new OrderAssignmentDto
                {
                    Order = order,
                    CertifiedDrivers = [],
                    CompatibleVehicles = [vehicle]
                }
                ]);
            }

            // Case 3: Vehicle Type Mismatch
            {
                var product = TestData.Product("P1", CertificationType.HospitalBeds, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);
                var driver = TestData.Driver("D1", [CertificationType.HospitalBeds], [VehicleType.Truck]);
                var vehicle = TestData.Vehicle("V1", VehicleType.Car); // mismatch

                var input = TestData.Input([order], [product], [driver], [vehicle]);

                Add("Case 3 - Vehicle Type Mismatch", input, [
                    new OrderAssignmentDto
                {
                    Order = order,
                    CertifiedDrivers = [driver],
                    CompatibleVehicles = [] // no truck available
                }
                ]);
            }

            // Case 4: Mixed Product Vehicle Requirements
            {
                var productA = TestData.Product("P1", CertificationType.HospitalBeds, [VehicleType.Car, VehicleType.Truck]);
                var productB = TestData.Product("P2", CertificationType.HospitalBeds, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1", "P2"]);
                var driver = TestData.Driver("D1", [CertificationType.HospitalBeds], [VehicleType.Truck]);
                var truck = TestData.Vehicle("V1", VehicleType.Truck);
                var car = TestData.Vehicle("V2", VehicleType.Car);

                var input = TestData.Input([order], [productA, productB], [driver], [truck, car]);

                Add("Case 4 - Mixed Product Vehicle Requirements", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver],
            CompatibleVehicles = [truck] // intersection = Truck only
        }
                ]);
            }

            // Case 5: Driver Cert Match but Not Allowed Vehicle
            {
                var product = TestData.Product("P1", CertificationType.Complex, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);
                var driver = TestData.Driver("D1", [CertificationType.Complex], [VehicleType.Car]); // not allowed to drive Truck
                var truck = TestData.Vehicle("V1", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver], [truck]);

                Add("Case 5 - Driver Cert Match but Not Allowed Vehicle", input, [
                    new OrderAssignmentDto
                    {
                        Order = order,
                        CertifiedDrivers = [driver],        // included based on certs only
                        CompatibleVehicles = [truck]        // truck matches vehicle type requirements
                    }
                ]);
            }

            // Case 6: Vehicle Match but No Certified Driver
            {
                var product = TestData.Product("P1", CertificationType.Respiratory, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);
                var driver = TestData.Driver("D1", [CertificationType.Basic], [VehicleType.Truck]); // wrong cert
                var truck = TestData.Vehicle("V1", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver], [truck]);

                Add("Case 6 - Vehicle Match but No Certified Driver", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [], // not qualified
            CompatibleVehicles = [truck]
        }
                ]);
            }


            // Case 7: All Valid (Multiple Drivers & Vehicles)
            {
                var product = TestData.Product("P1", CertificationType.Basic, [VehicleType.Car, VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);

                var driver1 = TestData.Driver("D1", [CertificationType.Basic], [VehicleType.Car, VehicleType.Truck]);
                var driver2 = TestData.Driver("D2", [CertificationType.Basic], [VehicleType.Car]);

                var vehicle1 = TestData.Vehicle("V1", VehicleType.Car);
                var vehicle2 = TestData.Vehicle("V2", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver1, driver2], [vehicle1, vehicle2]);

                Add("Case 7 - All Valid (Multiple Drivers & Vehicles)", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver1, driver2],
            CompatibleVehicles = [vehicle1, vehicle2]
        }
                ]);
            }

            // Case 8: Multiple Products with Shared Constraints
            {
                var product1 = TestData.Product("P1", CertificationType.Mobility, [VehicleType.Car, VehicleType.Truck]);
                var product2 = TestData.Product("P2", CertificationType.Mobility, [VehicleType.Car, VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1", "P2"]);

                var driver = TestData.Driver("D1", [CertificationType.Mobility], [VehicleType.Car, VehicleType.Truck]);
                var car = TestData.Vehicle("V1", VehicleType.Car);
                var truck = TestData.Vehicle("V2", VehicleType.Truck);

                var input = TestData.Input([order], [product1, product2], [driver], [car, truck]);

                Add("Case 8 - Multiple Products with Shared Constraints", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver],
            CompatibleVehicles = [car, truck]
        }
                ]);
            }

            // Case 9: Multiple Products with Disjoint Vehicle Types
            {
                var product1 = TestData.Product("P1", CertificationType.HospitalBeds, [VehicleType.Truck]);
                var product2 = TestData.Product("P2", CertificationType.HospitalBeds, [VehicleType.Car]);
                var order = TestData.Order("O1", ["P1", "P2"]);

                var driver = TestData.Driver("D1", [CertificationType.HospitalBeds], [VehicleType.Car, VehicleType.Truck]);
                var car = TestData.Vehicle("V1", VehicleType.Car);
                var truck = TestData.Vehicle("V2", VehicleType.Truck);

                var input = TestData.Input([order], [product1, product2], [driver], [car, truck]);

                Add("Case 9 - Disjoint Vehicle Types", input, [
                    new OrderAssignmentDto
                {
                    Order = order,
                    CertifiedDrivers = [driver],
                    CompatibleVehicles = [] // no vehicle supports both products
                }
                ]);
            }

            // Case 10: Empty Inputs
            {
                var input = TestData.Input([], [], [], []);
                Add("Case 10 - Empty Inputs", input, []);
            }
            // Case 11: Partial Certification Match
            {
                var product1 = TestData.Product("P1", CertificationType.Mobility, [VehicleType.Truck]);
                var product2 = TestData.Product("P2", CertificationType.Complex, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1", "P2"]);

                var driver = TestData.Driver("D1", [CertificationType.Mobility], [VehicleType.Truck]); // missing Complex

                var truck = TestData.Vehicle("V1", VehicleType.Truck);

                var input = TestData.Input([order], [product1, product2], [driver], [truck]);

                Add("Case 11 - Partial Certification Match", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [], // missing one cert
            CompatibleVehicles = [truck]
        }
                ]);
            }

            // Case 12: Duplicate Product IDs in Order
            {
                var product = TestData.Product("P1", CertificationType.Respiratory, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1", "P1"]); // duplicated

                var driver = TestData.Driver("D1", [CertificationType.Respiratory], [VehicleType.Truck]);
                var truck = TestData.Vehicle("V1", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver], [truck]);

                Add("Case 12 - Duplicate Product IDs in Order", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver],
            CompatibleVehicles = [truck]
        }
                ]);
            }

            // Case 13: Product With Multiple Vehicle Types
            {
                var product = TestData.Product("P1", CertificationType.Basic, [VehicleType.Car, VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);

                var driver = TestData.Driver("D1", [CertificationType.Basic], [VehicleType.Car, VehicleType.Truck]);
                var car = TestData.Vehicle("V1", VehicleType.Car);
                var truck = TestData.Vehicle("V2", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver], [car, truck]);

                Add("Case 13 - Product With Multiple Vehicle Types", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver],
            CompatibleVehicles = [car, truck]
        }
                ]);
            }

            // Case 14: Product With No Vehicle Type Specified
            {
                var product = TestData.Product("P1", CertificationType.Basic, []); // No vehicle types
                var order = TestData.Order("O1", ["P1"]);

                var driver = TestData.Driver("D1", [CertificationType.Basic], [VehicleType.Car]);
                var car = TestData.Vehicle("V1", VehicleType.Car);

                var input = TestData.Input([order], [product], [driver], [car]);

                Add("Case 14 - Product With No Vehicle Type Specified", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver],
            CompatibleVehicles = [] // nothing matches an empty vehicle type intersection
        }
                ]);
            }

            // Case 15: Multiple Drivers, One With Full Match
            {
                var product = TestData.Product("P1", CertificationType.Complex, [VehicleType.Truck]);
                var order = TestData.Order("O1", ["P1"]);

                var driver1 = TestData.Driver("D1", [CertificationType.Complex], [VehicleType.Truck]); // valid
                var driver2 = TestData.Driver("D2", [CertificationType.Basic], [VehicleType.Truck]);   // invalid

                var truck = TestData.Vehicle("V1", VehicleType.Truck);

                var input = TestData.Input([order], [product], [driver1, driver2], [truck]);

                Add("Case 15 - Multiple Drivers, One With Full Match", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver1],
            CompatibleVehicles = [truck]
        }
                ]);
            }

            // Case 16: Drivers With No Allowed Vehicles
            {
                var product = TestData.Product("P1", CertificationType.HospitalBeds, [VehicleType.Car]);
                var order = TestData.Order("O1", ["P1"]);

                var driver = TestData.Driver("D1", [CertificationType.HospitalBeds], []); // can't drive anything

                var car = TestData.Vehicle("V1", VehicleType.Car);

                var input = TestData.Input([order], [product], [driver], [car]);

                Add("Case 16 - Drivers With No Allowed Vehicles", input, [
                    new OrderAssignmentDto
        {
            Order = order,
            CertifiedDrivers = [driver],  // still included (cert match only)
            CompatibleVehicles = [car]    // separate from driver's ability to use it
        }
                ]);
            }


        }
    }
}