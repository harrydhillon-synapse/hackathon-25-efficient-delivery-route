using System.Text.Json;
using Synapse.DeliveryRoutes.Application.Models;
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

    [Theory]
    [InlineData(DataSet.Test)]
    [InlineData(DataSet.SimpleDemo)]
    [InlineData(DataSet.ComplexDemo)]
    public void LoadAllData(DataSet dataSet)
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData(dataSet);
        Assert.NotNull(inputData);
        
        _testOutputHelper.WriteLine($"{JsonSerializer.Serialize(inputData)}");
    }
}