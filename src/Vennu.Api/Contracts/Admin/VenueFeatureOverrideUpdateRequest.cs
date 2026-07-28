namespace Vennu.Api.Contracts.Admin;

public sealed record VenueFeatureOverrideUpdateRequest(bool Enabled, string Reason, DateTime? ExpiresAt);
