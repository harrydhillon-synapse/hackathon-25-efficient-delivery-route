using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public class Product
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required Dimensions Dimensions { get; set; }
    public required DeliveryRequirements DeliveryRequirements { get; set; }
    public required Billing Billing { get; set; }
}

public class Dimensions
{
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
}

public class DeliveryRequirements
{
    public required string SpecialHandling { get; set; }
    public required PackagingType PackagingType { get; set; }
    public required TransportRequirements TransportRequirements { get; set; }
    public required SetupAssistanceLevel SetupAssistance { get; set; }
    public List<int> HistoricalSetupTimes { get; set; } = [];
}

public class TransportRequirements
{
    public bool TemperatureControlled { get; set; }
    public required Orientation Orientation { get; set; }
    public required StackingLimit StackingLimit { get; set; }
}

public class Billing
{
    public decimal BasePrice { get; set; }
    public RentalRate RentalRate { get; set; } = new();
}

public class RentalRate
{
    public decimal Daily { get; set; }
    public decimal Weekly { get; set; }
    public decimal Monthly { get; set; }
}

public enum Orientation
{
    [Description("Upright Position Preferred")]
    UprightPositionPreferred = 1,

    [Description("Upright Position")]
    UprightPosition = 2,

    [Description("Upright Position Required")]
    UprightPositionRequired = 3,

    [Description("Any Position")]
    AnyPosition = 4
}

public enum StackingLimit
{
    [Description("1 Unit")]
    OneUnit = 1,

    [Description("2 Units")]
    TwoUnits = 2,

    [Description("3 Units")]
    ThreeUnits = 3,

    [Description("4 Units")]
    FourUnits = 4,

    [Description("5 Units")]
    FiveUnits = 5,

    [Description("Not Stackable")]
    NotStackable = 6
}

public enum SetupAssistanceLevel
{
    [Description("Level 1 - Basic Assembly Required")]
    Level1BasicAssemblyRequired = 1,

    [Description("Level 2 - Basic Training Required")]
    Level2BasicTrainingRequired = 2,

    [Description("Level 2 - Basic Assembly Required")]
    Level2BasicAssemblyRequired = 3,

    [Description("Level 3 - Professional Installation Required")]
    Level3ProfessionalInstallationRequired = 4
}

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
