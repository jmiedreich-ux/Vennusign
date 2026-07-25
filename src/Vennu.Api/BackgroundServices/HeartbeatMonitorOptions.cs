namespace Vennu.Api.BackgroundServices;

public sealed class HeartbeatMonitorOptions
{
    public const string SectionName = "HeartbeatMonitor";

    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);

    public TimeSpan StaleThreshold { get; set; } = TimeSpan.FromSeconds(90);
}
