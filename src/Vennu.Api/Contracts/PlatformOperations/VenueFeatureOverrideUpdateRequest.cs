namespace Vennu.Api.Contracts.PlatformOperations;

public sealed record VenueFeatureOverrideUpdateRequest(bool Enabled, string Reason, DateTime? ExpiresAt);
