using System.Collections.ObjectModel;
using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface ICapabilityDecisionEngine
{
    CapabilityDecisionResult Evaluate(CapabilityDecisionInput input);
    IReadOnlyCollection<CapabilityDecisionResult> EvaluateBatch(IEnumerable<CapabilityDecisionInput> inputs);
}

public sealed class CapabilityDecisionEngine : ICapabilityDecisionEngine
{
    private static readonly CapabilityDecisionCategory[] RequiredDimensions =
    [
        CapabilityDecisionCategory.IdentityContext,
        CapabilityDecisionCategory.Rollout,
        CapabilityDecisionCategory.Entitlement,
        CapabilityDecisionCategory.Permission,
        CapabilityDecisionCategory.AddOn,
        CapabilityDecisionCategory.Allowance,
        CapabilityDecisionCategory.ResourceState,
        CapabilityDecisionCategory.RequestValidity
    ];

    public CapabilityDecisionResult Evaluate(CapabilityDecisionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.CorrelationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.Locale);

        if (!Version1CapabilityRegistry.ById.ContainsKey(input.Capability))
        {
            return Blocked(
                input,
                CapabilityDecisionOutcome.Unavailable,
                CapabilityDecisionCategory.Capability,
                "capability.unknown",
                "decisions.capability.unknown",
                resolution: null);
        }

        var grouped = input.Dimensions
            .GroupBy(item => item.Category)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var missingOrDuplicate = RequiredDimensions
            .Where(category => !grouped.TryGetValue(category, out var values) || values.Length != 1)
            .Select(category => category.ToString())
            .ToArray();
        if (missingOrDuplicate.Length > 0)
        {
            return Blocked(
                input,
                CapabilityDecisionOutcome.Unavailable,
                CapabilityDecisionCategory.Capability,
                "decision.input_incomplete",
                "decisions.input_incomplete",
                new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
                {
                    ["dimensions"] = string.Join(",", missingOrDuplicate)
                }),
                "retry_with_complete_context");
        }

        foreach (var category in RequiredDimensions)
        {
            var dimension = grouped[category][0];
            ValidateDimension(dimension);
            if (dimension.Status == CapabilityDimensionStatus.Failed)
            {
                return Blocked(
                    input,
                    dimension.FailureOutcome!.Value,
                    dimension.Category,
                    dimension.ReasonCode,
                    dimension.MessageKey,
                    dimension.Parameters,
                    dimension.Resolution,
                    dimension.RetryAfter);
            }
        }

        var conditions = RequiredDimensions
            .Select(category => grouped[category][0])
            .Where(item => item.Status == CapabilityDimensionStatus.Condition)
            .Select(item => new CapabilityDecisionCondition(
                item.Category,
                item.ReasonCode,
                item.MessageKey,
                item.Parameters ?? CapabilityDecisionResult.EmptyParameters,
                item.Resolution))
            .ToArray();

        return new CapabilityDecisionResult(
            conditions.Length == 0 ? CapabilityDecisionOutcome.Allowed : CapabilityDecisionOutcome.AllowedWithConditions,
            conditions.Length == 0 ? "decision.allowed" : "decision.allowed_with_conditions",
            CapabilityDecisionCategory.None,
            input.Capability,
            conditions.Length == 0 ? "decisions.allowed" : "decisions.allowed_with_conditions",
            CapabilityDecisionResult.EmptyParameters,
            input.CorrelationId,
            input.Locale,
            null,
            null,
            conditions);
    }

    public IReadOnlyCollection<CapabilityDecisionResult> EvaluateBatch(IEnumerable<CapabilityDecisionInput> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        return Array.AsReadOnly(inputs.Select(Evaluate).ToArray());
    }

    private static CapabilityDecisionResult Blocked(
        CapabilityDecisionInput input,
        CapabilityDecisionOutcome outcome,
        CapabilityDecisionCategory category,
        string reasonCode,
        string messageKey,
        IReadOnlyDictionary<string, string>? parameters = null,
        string? resolution = null,
        TimeSpan? retryAfter = null) =>
        new(
            outcome,
            reasonCode,
            category,
            input.Capability,
            messageKey,
            parameters ?? CapabilityDecisionResult.EmptyParameters,
            input.CorrelationId,
            input.Locale,
            resolution,
            retryAfter,
            []);

    private static void ValidateDimension(CapabilityDecisionDimension dimension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dimension.ReasonCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(dimension.MessageKey);
        if (dimension.Status == CapabilityDimensionStatus.Failed && dimension.FailureOutcome is null)
        {
            throw new ArgumentException("A failed capability dimension requires a failure outcome.", nameof(dimension));
        }
    }
}
