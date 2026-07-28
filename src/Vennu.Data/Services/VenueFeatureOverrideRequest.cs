namespace Vennu.Data.Services;

public sealed record VenueFeatureOverrideRequest(bool Enabled, string Reason, DateTime? ExpiresAt);
