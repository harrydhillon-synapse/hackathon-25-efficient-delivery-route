using System.Text;

namespace Synapse.DeliveryRoutes.Application.Models;

public class Schedule
{
    public required bool Successful { get; set; }
    public required DriverSchedule[]? DriverSchedules { get; set; }

    public override string ToString()
    {
        if (!Successful)
        {
            return "Unable to create schedule";
        }

        var output = new StringBuilder();

        foreach (var driverSchedule in DriverSchedules!)
        {
            output.AppendLine(driverSchedule.ToString());
        }

        return output.ToString();
    }
}