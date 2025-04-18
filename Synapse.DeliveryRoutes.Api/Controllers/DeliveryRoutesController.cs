using Microsoft.AspNetCore.Mvc;
using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Services;
using System.Text.Json;
using Synapse.DeliveryRoutes.Application.ViewModels;

namespace Synapse.DeliveryRoutes.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DeliveryRoutesController : ControllerBase
{
    private readonly ILogger<DeliveryRoutesController> _logger;
    private const string OutputDirectory = "App_Data/Routes";

    public DeliveryRoutesController(ILogger<DeliveryRoutesController> logger)
    {
        _logger = logger;
    }

    [HttpPost("generate/{dataSet}")]
    public string GenerateRoutes(DataSet dataSet)
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData(dataSet);
        var result = new Scheduler(inputData).CreateSchedule();

        var routes = Utilities.ConvertToDeliveryRoutes(result, inputData);

        Directory.CreateDirectory(OutputDirectory);

        var filePath = Path.Combine(OutputDirectory, $"result.json");
        var json = JsonSerializer.Serialize(routes, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(filePath, json);

        return "OK";
    }

    [HttpGet()]
    public IEnumerable<DeliveryProductViewModel> Get()
    {
        var filePath = Path.Combine(OutputDirectory, $"result.json");

        if (!System.IO.File.Exists(filePath))
        {
            return Array.Empty<DeliveryProductViewModel>();
        }

        var json = System.IO.File.ReadAllText(filePath);
        var routes = JsonSerializer.Deserialize<DeliveryProductViewModel[]>(json) ?? [];

        return routes;
    }
}
