using Synapse.DeliveryRoutes.Application.Models;
using Synapse.DeliveryRoutes.Application.Services;
using Xunit.Abstractions;

namespace Synapse.DeliveryRoutes.Tests;

public class SchedulerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SchedulerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
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