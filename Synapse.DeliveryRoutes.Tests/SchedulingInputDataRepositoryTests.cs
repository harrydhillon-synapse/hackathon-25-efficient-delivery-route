using Synapse.DeliveryRoutes.Application.Services;

namespace Synapse.DeliveryRoutes.Tests;

public class SchedulingInputDataRepositoryTests
{
    [Fact]
    public void LoadAllData()
    {
        var inputData = new SchedulingInputDataRepository().LoadAllData();
        Assert.NotNull(inputData);
    }
}