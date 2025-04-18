using Microsoft.AspNetCore.Mvc;
using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Services;
using System.Text.Json;
using Synapse.DeliveryRoutes.Application.ViewModels;

namespace Synapse.DeliveryRoutes.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")] // Ensures all actions default to JSON
public class DeliveryRoutesController : ControllerBase
{
    private readonly ILogger<DeliveryRoutesController> _logger;
    private const string OutputDirectory = "App_Data/Routes";

    public DeliveryRoutesController(ILogger<DeliveryRoutesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates optimized DME delivery routes using a selected predefined dataset.
    /// </summary>
    /// <remarks>
    /// This endpoint loads one of three predefined input datasets and computes the most efficient delivery routes 
    /// using Google OR-Tools.
    ///
    /// Each dataset includes:
    /// - <b>Delivery Technicians</b>: Their availability, vehicle qualifications (car or truck), and certifications required for DME setup (e.g., Respiratory, Mobility).
    /// - <b>Vehicles</b>: A list of available delivery vehicles, including type (car or truck).
    /// - <b>Orders</b>: Contain product information, delivery address, delivery window, setup time, whether a truck is required, and any required technician certifications.
    ///
    /// The SmartScheduler assigns orders to appropriate technicians and vehicles, ensuring:
    /// - Only certified technicians are assigned to orders that require specific DME expertise.
    /// - Orders that require a truck are matched with technicians who can operate trucks.
    /// - Total driving time and delivery distance are minimized.
    ///
    /// The response indicates whether the scheduling succeeded and how many delivery routes were generated.
    /// </remarks>
    /// <param name="dataSet">
    /// The name of the predefined dataset to use:
    /// - <b>Test</b>: A minimal dataset for internal validation.
    /// - <b>SimpleDemo</b>: A small demo dataset suitable for walkthroughs.
    /// - <b>ComplexDemo</b>: A full operational dataset with real-world complexity.
    /// </param>
    /// <returns>A result object indicating success and number of generated routes.</returns>
    [HttpPost("generate/{dataSet}")]
    [ProducesResponseType(typeof(GenerateDeliveryRoutesResponse), StatusCodes.Status200OK)]
    public GenerateDeliveryRoutesResponse GenerateRoutes(DataSet dataSet)
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData(dataSet);
        var result = new Scheduler(inputData).CreateSchedule();

        var routes = Utilities.ConvertToDeliveryRoutes(result, inputData);

        Directory.CreateDirectory(OutputDirectory);

        var filePath = Path.Combine(OutputDirectory, $"result.json");
        var json = JsonSerializer.Serialize(routes, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(filePath, json);

        var response = new GenerateDeliveryRoutesResponse
        {
            Result = result.Successful
                ? GenerateDeliveryRoutesResult.Succeeded
                : GenerateDeliveryRoutesResult.Failed,
            RouteCount = result.Successful
                ? routes.Length
                : null
        };

        return response;
    }

    /// <summary>
    /// Retrieves the most recently generated delivery routes.
    /// </summary>
    /// <remarks>
    /// This endpoint reads the previously generated route data and returns the full set of delivery routes as an array.
    ///
    /// Each route includes:
    /// - <b>Delivery Technician</b>
    /// - <b>Assigned Vehicle</b>
    /// - <b>Orders</b> for each route, including navigation information, patient information, product information, and delivery notes
    ///
    /// If no route data has been generated yet, an empty list is returned.
    /// </remarks>
    /// <returns>
    /// A collection of <c>DeliveryRouteViewModel</c> objects representing the last scheduled delivery plan.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeliveryRouteViewModel>), StatusCodes.Status200OK)]
    public IEnumerable<DeliveryRouteViewModel> Get()
    {
        var filePath = Path.Combine(OutputDirectory, $"result.json");

        if (!System.IO.File.Exists(filePath))
        {
            return Array.Empty<DeliveryRouteViewModel>();
        }

        var json = System.IO.File.ReadAllText(filePath);
        var routes = JsonSerializer.Deserialize<DeliveryRouteViewModel[]>(json) ?? [];

        return routes;
    }
}
