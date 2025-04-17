using System.Text;

namespace Synapse.DeliveryRoutes.Application.Models;

public class DriverSchedule
{
    public required Driver Driver { get; set; }
    public required Vehicle Vehicle { get; set; }
    public required Order[] Orders { get; set; }
    public bool StartAtDepot { get; set; } = true;
    public bool EndAtDepot { get; set; } = true;

    public override string ToString()
    {
        if (Orders?.Any() != true)
        {
            return $"{Driver.Name} has no deliveries.";
        }

        var output = new StringBuilder();
        output.AppendLine($"{Driver.Name} (Vehicle {Vehicle.Id} - {Vehicle.Type}) has {Orders.Length} deliveries");
        for (int deliveryIndex = 0; deliveryIndex < Orders.Length; deliveryIndex++)
        {
            output.Append($"    {deliveryIndex + 1}) ");
            var order = Orders[deliveryIndex];
            output.AppendLine($"Order {order.Id} for patient {order.PatientName} with {order.ProductIds.Count} products at: {order.Address}");
        }

        return output.ToString();
    }
}
