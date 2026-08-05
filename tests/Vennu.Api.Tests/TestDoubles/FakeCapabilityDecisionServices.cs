using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.TestDoubles;

internal sealed class FakeCapabilityDecisionServices(params string[] allowedCapabilities)
    : ICapabilityActionAuthorizer, ICapabilityDecisionService
{
    private readonly HashSet<string> allowed = new(allowedCapabilities, StringComparer.Ordinal);

    public Task<CapabilityDecisionResult> RequireAllowedAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        var result = Result(capability, correlationId, locale);
        return result.IsAllowed
            ? Task.FromResult(result)
            : throw new CapabilityDecisionDeniedException(result);
    }

    public Task<CapabilityDecisionResult> EvaluateAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result(capability, correlationId, locale));

    public Task<IReadOnlyCollection<CapabilityDecisionResult>> EvaluateBatchAsync(
        IReadOnlyCollection<CapabilityId> capabilities,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<CapabilityDecisionResult>>(
            capabilities.Select(capability => Result(capability, correlationId, locale)).ToArray());

    private CapabilityDecisionResult Result(CapabilityId capability, string correlationId, string locale)
    {
        var isAllowed = allowed.Contains(capability.Value);
        return new CapabilityDecisionResult(
            isAllowed ? CapabilityDecisionOutcome.Allowed : CapabilityDecisionOutcome.Denied,
            isAllowed ? "decision.allowed" : "permission.required",
            isAllowed ? CapabilityDecisionCategory.None : CapabilityDecisionCategory.Permission,
            capability,
            isAllowed ? "decisions.allowed" : "decisions.permission.required",
            CapabilityDecisionResult.EmptyParameters,
            correlationId,
            locale,
            isAllowed ? null : "ask_scope_administrator",
            null,
            []);
    }
}
