using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum PackagingType
{
    [Description("Original Manufacturer Box")]
    OriginalManufacturerBox = 1,

    [Description("Original Manufacturer Box With Foam Padding")]
    OriginalManufacturerBoxWithFoamPadding = 2,

    [Description("Original Manufacturer Case")]
    OriginalManufacturerCase = 3,

    [Description("Disassembled In Multiple Boxes")]
    DisassembledInMultipleBoxes = 4,

    [Description("Partially Disassembled")]
    PartiallyDisassembled = 5
}