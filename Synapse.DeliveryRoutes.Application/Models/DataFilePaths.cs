namespace Synapse.DeliveryRoutes.Application.Models;

public class DataFilePaths(DataSet dataSet)
{
    public string ProductsFile => $"Data/{dataSet.ToString().ToLowerInvariant()}/products.json";
    public string OrdersFile => $"Data/{dataSet.ToString().ToLowerInvariant()}/orders.json";
    public string DriversFile => $"Data/{dataSet.ToString().ToLowerInvariant()}/drivers.json";
    public string VehiclesFile => $"Data/{dataSet.ToString().ToLowerInvariant()}/vehicles.json";
    public string OfficeFile => $"Data/{dataSet.ToString().ToLowerInvariant()}/office.json";
}