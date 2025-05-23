﻿namespace Synapse.DeliveryRoutes.Application.Models;

public class SchedulingInputData
{
    public List<Product> Products { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public List<Driver> Drivers { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public required Office Office { get; set; }
}