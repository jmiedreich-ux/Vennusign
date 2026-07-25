namespace Vennu.Data.Services;

public sealed record FeatureEntitlement(
    string Key,
    bool Enabled,
    string? LimitValue,
    string Source);
