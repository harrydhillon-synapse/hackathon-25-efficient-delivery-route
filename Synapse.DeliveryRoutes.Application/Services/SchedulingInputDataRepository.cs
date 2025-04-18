using System.Text.Json;
using System.Text.Json.Serialization;
using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.Services;

public class SchedulingInputDataRepository
{
    // Method to load all data
    public SchedulingInputData LoadAllData(DataSet dataSet)
    {
        var options = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new DateOnlyJsonConverter(),
                new TimeOnlyJsonConverter()
            },
            PropertyNameCaseInsensitive = true
        };
        var dataFilePaths = new DataFilePaths(dataSet);

        var productsJson = File.ReadAllText(dataFilePaths.ProductsFile);
        var products = JsonSerializer.Deserialize<List<Product>>(productsJson, options);

        var ordersJson = File.ReadAllText(dataFilePaths.OrdersFile);
        var orders = JsonSerializer.Deserialize<List<Order>>(ordersJson, options);

        var driversJson = File.ReadAllText(dataFilePaths.DriversFile);
        var drivers = JsonSerializer.Deserialize<List<Driver>>(driversJson, options);

        var vehiclesJson = File.ReadAllText(dataFilePaths.VehiclesFile);
        var vehicles = JsonSerializer.Deserialize<List<Vehicle>>(vehiclesJson, options);

        var officeJson = File.ReadAllText(dataFilePaths.OfficeFile);
        var office = JsonSerializer.Deserialize<Office>(officeJson, options);

        return new SchedulingInputData
        {
            Products = products!,
            Orders = orders!,
            Drivers = drivers!,
            Vehicles = vehicles!,
            Office = office!
        };
    }

    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateOnly.ParseExact(reader.GetString()!, Format);

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Format));
    }

    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string Format = "HH:mm";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => TimeOnly.ParseExact(reader.GetString()!, Format);

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Format));
    }
}
