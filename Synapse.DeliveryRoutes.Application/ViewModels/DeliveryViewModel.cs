using Synapse.DeliveryRoutes.Application.Models;

namespace Synapse.DeliveryRoutes.Application.ViewModels;

public class DeliveryViewModel
{
    public required string OrderId { get; set; }
    public required string PatientName { get; set; }
    public required string PatientPhone { get; set; }
    public required string Address { get; set; }
    public required GeoCoordinates Location { get; set; }
    public required DeliveryWindowViewModel DeliveryWindow { get; set; }
    public required string ExpectedArrivalTime { get; set; }
    public required int ExpectedDriveTimeMinutes { get; set; }
    public required int ExpectedSetupTimeMinutes { get; set; }
    public required string ExpectedFinishTime { get; set; }
    public required string DriveTimeDescription { get; set; }
    public required List<DeliveryProductViewModel> Products { get; set; }
    public required List<string> Notes { get; set; }
}