using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

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