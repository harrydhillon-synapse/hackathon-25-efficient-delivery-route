using Synapse.DeliveryRoutes.Application.Services;
using Xunit.Abstractions;

namespace Synapse.DeliveryRoutes.Tests;

public class ScheduleSolverTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ScheduleSolverTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SolveSchedule()
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData();
        var result = new ScheduleSolver().SolveSchedule(inputData);
        Assert.NotNull(result);
        Assert.True(result.Successful);
        
        _testOutputHelper.WriteLine($"Schedule Result: {result.DebugOutput}");
    }
}