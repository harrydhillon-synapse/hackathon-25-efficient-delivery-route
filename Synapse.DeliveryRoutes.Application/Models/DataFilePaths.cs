namespace Synapse.DeliveryRoutes.Application.Models;

public class DataFilePaths(DataSet dataSet)
{
    private static string BasePath => AppContext.BaseDirectory;
    public string ProductsFile => $"{BasePath}/Data/{dataSet.ToString().ToLowerInvariant()}/products.json";
    public string OrdersFile => $"{BasePath}/Data/{dataSet.ToString().ToLowerInvariant()}/orders.json";
    public string DriversFile => $"{BasePath}/Data/{dataSet.ToString().ToLowerInvariant()}/drivers.json";
    public string VehiclesFile => $"{BasePath}/Data/{dataSet.ToString().ToLowerInvariant()}/vehicles.json";
    public string OfficeFile => $"{BasePath}/Data/{dataSet.ToString().ToLowerInvariant()}/office.json";
}