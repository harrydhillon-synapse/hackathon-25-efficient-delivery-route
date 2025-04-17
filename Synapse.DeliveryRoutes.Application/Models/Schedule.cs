using System.Text;

namespace Synapse.DeliveryRoutes.Application.Models;

public class Schedule
{
    public required bool Successful { get; set; }
    public required DriverSchedule[]? DriverSchedules { get; set; }
}