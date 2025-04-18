namespace Synapse.DeliveryRoutes.Application.Services;

public static class Settings
{
    public const int TimeLimitInSeconds = 5;
    public const double ScaleFactor = 1;
    public const int MinutesPerWorkday = AfternoonEndMinute - 1;
    public const double HistoricalSetupTimesStandardDeviationTunableMultiple  = 0.4;
    public const int MinimumDeliveryTimeMinutes = 15;
    public const int MinimumHistoricalSetupTimesToApplyStandardDeviation = 5;
    public const int DrivingSpeedKmPerHour = 40;
    public const int MorningStartMinute = 0;
    public const int MorningEndMinute = 239;
    public const int AfternoonStartMinute = 240;
    public const int AfternoonEndMinute = 480;
    public const int BreakTimeBetweenAppointments = 0;
    public static readonly TimeSpan MorningStartTime = new TimeSpan(9, 0, 0);
}