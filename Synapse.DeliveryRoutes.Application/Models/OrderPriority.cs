﻿using System.ComponentModel;

namespace Synapse.DeliveryRoutes.Application.Models;

public enum OrderPriority
{
    [Description("Low")]
    Low = 1,

    [Description("Medium")]
    Medium = 2,

    [Description("High")]
    High = 3
}