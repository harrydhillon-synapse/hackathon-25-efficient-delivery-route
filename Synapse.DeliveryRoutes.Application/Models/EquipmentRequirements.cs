namespace Synapse.DeliveryRoutes.Application.Models;

public class EquipmentRequirements
{
    public int MinimumSetupTime { get; set; } // in minutes
    public string? MaintenanceSchedule { get; set; }
    public List<string> CleaningRequirements { get; set; } = [];
    public List<string> TestingProcedures { get; set; } = [];
}