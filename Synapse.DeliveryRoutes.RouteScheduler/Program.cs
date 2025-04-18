using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Services;

try
{
    Console.WriteLine("Loading input data...");
    var inputData = new SchedulingInputDataRepository().LoadAllData(DataSet.Original);

    Console.WriteLine("Creating scheduler context...");
    var context = new SchedulerContext(inputData);

    Console.WriteLine("Solving schedule...");
    var result = new Scheduler(context).CreateSchedule();

    if (!result.Successful)
    {
        Console.WriteLine("Scheduler was unable to create a valid schedule.");
    }
    else
    {
        Console.WriteLine("Schedule solved successfully.\n");
        Console.WriteLine(Utilities.ToString(result, inputData.Products.ToArray()));
    }
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred during scheduling:");
    Console.WriteLine(ex.ToString());
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();