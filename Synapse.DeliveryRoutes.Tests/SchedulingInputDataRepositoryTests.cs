using System.Text.Json;
using Synapse.DeliveryRoutes.Application.Services;
using Xunit.Abstractions;

namespace Synapse.DeliveryRoutes.Tests;

public class SchedulingInputDataRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SchedulingInputDataRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void LoadAllData()
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData();
        Assert.NotNull(inputData);
        
        _testOutputHelper.WriteLine($"{JsonSerializer.Serialize(inputData)}");
    }
}