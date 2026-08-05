using System.Collections.ObjectModel;

namespace Vennu.Core.Models;

public enum CapabilityDecisionOutcome
{
    Allowed = 1,
    AllowedWithConditions = 2,
    Denied = 3,
    Unavailable = 4,
    TemporarilyBlocked = 5
}

public enum CapabilityDecisionCategory
{
    None = 0,
    IdentityContext = 1,
    Capability = 2,
    Rollout = 3,
    Entitlement = 4,
    Permission = 5,
    AddOn = 6,
    Allowance = 7,
    ResourceState = 8,
    RequestValidity = 9
}

public enum CapabilityDimensionStatus
{
    Satisfied = 1,
    Condition = 2,
    Failed = 3
}

public sealed record CapabilityDecisionDimension(
    CapabilityDecisionCategory Category,
    CapabilityDimensionStatus Status,
    string ReasonCode,
    string MessageKey,
    CapabilityDecisionOutcome? FailureOutcome = null,
    IReadOnlyDictionary<string, string>? Parameters = null,
    string? Resolution = null,
    TimeSpan? RetryAfter = null)
{
    public static CapabilityDecisionDimension Satisfied(CapabilityDecisionCategory category) =>
        new(category, CapabilityDimensionStatus.Satisfied, "decision.satisfied", "decisions.satisfied");

    public static CapabilityDecisionDimension Condition(
        CapabilityDecisionCategory category,
        string reasonCode,
        string messageKey,
        IReadOnlyDictionary<string, string>? parameters = null,
        string? resolution = null) =>
        new(category, CapabilityDimensionStatus.Condition, reasonCode, messageKey, null, parameters, resolution);

    public static CapabilityDecisionDimension Failed(
        CapabilityDecisionCategory category,
        CapabilityDecisionOutcome outcome,
        string reasonCode,
        string messageKey,
        IReadOnlyDictionary<string, string>? parameters = null,
        string? resolution = null,
        TimeSpan? retryAfter = null)
    {
        if (outcome is CapabilityDecisionOutcome.Allowed or CapabilityDecisionOutcome.AllowedWithConditions)
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), "A failed dimension requires a blocking outcome.");
        }

        return new(category, CapabilityDimensionStatus.Failed, reasonCode, messageKey, outcome, parameters, resolution, retryAfter);
    }
}

public sealed record CapabilityDecisionInput(
    CapabilityId Capability,
    IReadOnlyCollection<CapabilityDecisionDimension> Dimensions,
    string CorrelationId,
    string Locale = "en-US");

public sealed record CapabilityDecisionCondition(
    CapabilityDecisionCategory Category,
    string ReasonCode,
    string MessageKey,
    IReadOnlyDictionary<string, string> Parameters,
    string? Resolution);

public sealed record CapabilityDecisionResult(
    CapabilityDecisionOutcome Decision,
    string ReasonCode,
    CapabilityDecisionCategory Category,
    CapabilityId Capability,
    string MessageKey,
    IReadOnlyDictionary<string, string> Parameters,
    string CorrelationId,
    string Locale,
    string? Resolution,
    TimeSpan? RetryAfter,
    IReadOnlyCollection<CapabilityDecisionCondition> Conditions)
{
    public bool IsAllowed => Decision is CapabilityDecisionOutcome.Allowed or CapabilityDecisionOutcome.AllowedWithConditions;

    public static IReadOnlyDictionary<string, string> EmptyParameters { get; } =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
}
