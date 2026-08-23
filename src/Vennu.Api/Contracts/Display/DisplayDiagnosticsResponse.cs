namespace Vennu.Api.Contracts.Display;

/// <summary>
/// The server's half of a screen's diagnostic picture: identifiers, states and timestamps
/// only. Anonymous, like the other player endpoints - reachable by anyone who knows a screen
/// id - so it must never carry menu content, customer PII, or organisation detail.
/// </summary>
public class DisplayDiagnosticsResponse
{
    public Guid ScreenId { get; set; }

    public Guid? VenueId { get; set; }

    public string ScreenKey { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public bool IsAssignedToVenue { get; set; }

    public string Status { get; set; } = "Offline";

    public DateTime? LastSeenUtc { get; set; }

    public double? SecondsSinceLastSeen { get; set; }

    public bool IsStale { get; set; }

    public string? Platform { get; set; }

    public string? AppVersion { get; set; }

    public string? DesiredAppVersion { get; set; }

    public int ConfiguredWidthPixels { get; set; }

    public int ConfiguredHeightPixels { get; set; }

    public long? AuthoritativeRevision { get; set; }

    public long? AppliedRevision { get; set; }

    public string? DeliveryState { get; set; }

    public DateTime? DeliveryRequestedUtc { get; set; }

    public DateTime? DeliveryReceivedUtc { get; set; }

    public DateTime? DeliveryAppliedUtc { get; set; }

    public string? DeliveryFailureCode { get; set; }

    public string? LastReceiptPlayerVersion { get; set; }

    public string? LastReceiptShellVersion { get; set; }

    public bool IsOnboardingFirstScreen { get; set; }

    public DateTime? OnboardingGoLiveAchievedUtc { get; set; }
}
