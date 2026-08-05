namespace Vennu.Core.Models;

public enum CapabilityRolloutState
{
    Available = 1,
    Unavailable = 2,
    TemporarilyBlocked = 3
}

public sealed record CapabilityAccessPolicy(
    CapabilityId Capability,
    CapabilityRolloutState Rollout,
    bool Entitled,
    bool AddOnAttached,
    int? AllowanceLimit,
    int AllowanceUsed,
    DateTime? RetryAfterUtc = null)
{
    public bool AllowanceAvailable => AllowanceLimit is null || AllowanceUsed < AllowanceLimit;

    public static CapabilityAccessPolicy DefaultFor(CapabilityDefinition definition) => new(
        definition.Id,
        definition.Classification == CapabilityClassification.Deferred
            ? CapabilityRolloutState.Unavailable
            : CapabilityRolloutState.Available,
        definition.Classification is CapabilityClassification.UniversalCore or CapabilityClassification.Governance,
        definition.Classification != CapabilityClassification.AdvancedNative,
        null,
        0);
}
