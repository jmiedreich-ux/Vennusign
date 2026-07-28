namespace Vennu.Data.Services;

public sealed record VenueDirectoryItem(
    Guid VenueId,
    string Name,
    string Type,
    Guid? TierId,
    string? TierName,
    string SubscriptionStatus,
    int ScreenCount,
    DateTime? LastActiveUtc,
    int OverrideCount,
    string Health);

