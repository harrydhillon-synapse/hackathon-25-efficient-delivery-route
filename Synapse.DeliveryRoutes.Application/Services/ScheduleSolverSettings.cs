namespace Synapse.DeliveryRoutes.Application.Services;

public static class ScheduleSolverSettings
{
    public const int TimeLimitInSeconds = 7;
    public const double ScaleFactor = 1;
    public const int MinutesPerWorkday = 8 * 60;
    public const double HistoricalSetupTimesStandardDeviationTunableMultiple  = 0.4;
    public const int MinimumDeliveryTimeMinutes = 15;
    public const int MinimumHistoricalSetupTimesToApplyStandardDeviation = 5;
    public const int DrivingSpeedKmPerHour = 40;
}