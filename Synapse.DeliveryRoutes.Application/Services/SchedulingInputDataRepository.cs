﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Synapse.DeliveryRoutes.Application.Models.V1;

namespace Synapse.DeliveryRoutes.Application.Services;

public class SchedulingInputDataRepository
{
    // Properties for all data types

    // JSON file paths (relative to the output directory)
    private const string ProductsFile = "Data/V1/products.json";
    private const string SetupTimesFile = "Data/V1/setup.json";
    private const string OrdersFile = "Data/V1/orders.json";
    private const string DriversFile = "Data/V1/drivers.json";
    private const string VehiclesFile = "Data/V1/vehicles.json";
    private const string OfficeFile = "Data/V1/office.json";

    // Method to load all data
    public SchedulingInputData LoadAllData()
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        var productsJson = File.ReadAllText(ProductsFile);
        var products = JsonSerializer.Deserialize<List<Product>>(productsJson, options);

        var setupTimesJson = File.ReadAllText(SetupTimesFile);
        var setupTimes = JsonSerializer.Deserialize<ProductSetupTimes>(setupTimesJson, options);

        var ordersJson = File.ReadAllText(OrdersFile);
        var orders = JsonSerializer.Deserialize<List<Order>>(ordersJson, options);

        var driversJson = File.ReadAllText(DriversFile);
        var drivers = JsonSerializer.Deserialize<List<Driver>>(driversJson, options);

        var vehiclesJson = File.ReadAllText(VehiclesFile);
        var vehicles = JsonSerializer.Deserialize<List<Vehicle>>(vehiclesJson, options);

        var officeJson = File.ReadAllText(OfficeFile);
        var office = JsonSerializer.Deserialize<Office>(officeJson, options);

        return new SchedulingInputData
        {
            Products = products!,
            SetupTimes = setupTimes!.Products,
            Orders = orders!,
            Drivers = drivers!,
            Vehicles = vehicles!,
            Office = office!
        };
    }
}
