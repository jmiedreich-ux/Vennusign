namespace Vennu.Core.Models;

public sealed class HappyHourSchedule
{
    public Guid VenueId { get; set; }

    public TimeSpan StartLocalTime { get; set; } = TimeSpan.FromHours(16);

    public TimeSpan EndLocalTime { get; set; } = TimeSpan.FromHours(19);

    public int ActiveDaysMask { get; set; } = 127;

    public bool IsEnabled { get; set; } = true;

    public string OverrideMode { get; set; } = HappyHourOverrideMode.Automatic;

    public DateTime UpdatedUtc { get; set; }
}

public static class HappyHourOverrideMode
{
    public const string Automatic = "automatic";
    public const string ForceOn = "force_on";
    public const string ForceOff = "force_off";

    public static string Normalize(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            Automatic or null or "" => Automatic,
            ForceOn => ForceOn,
            ForceOff => ForceOff,
            _ => throw new ArgumentException("Override mode must be automatic, force_on, or force_off.", nameof(value))
        };
}
