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

    [Fact]
    public void SolveSchedule()
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData();
        var context = new SchedulerContext(inputData);
        var result = new Scheduler().CreateSchedule(context);
        Assert.NotNull(result);
        Assert.True(result.Successful);
        
        _testOutputHelper.WriteLine($"Schedule:");
        _testOutputHelper.WriteLine($"{result}");
    }
}