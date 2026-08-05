using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface ICapabilityDecisionInputProvider
{
    Task<CapabilityDecisionInput> ResolveAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CapabilityDecisionInput>> ResolveBatchAsync(
        IReadOnlyCollection<CapabilityId> capabilities,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default);
}

public interface ICapabilityDecisionService
{
    Task<CapabilityDecisionResult> EvaluateAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CapabilityDecisionResult>> EvaluateBatchAsync(
        IReadOnlyCollection<CapabilityId> capabilities,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default);
}

public sealed class CapabilityDecisionService(
    ICapabilityDecisionInputProvider inputs,
    ICapabilityDecisionEngine engine) : ICapabilityDecisionService
{
    public async Task<CapabilityDecisionResult> EvaluateAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        var input = await inputs.ResolveAsync(capability, correlationId, locale, cancellationToken).ConfigureAwait(false);
        return engine.Evaluate(input);
    }

    public async Task<IReadOnlyCollection<CapabilityDecisionResult>> EvaluateBatchAsync(
        IReadOnlyCollection<CapabilityId> capabilities,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        var resolved = await inputs.ResolveBatchAsync(capabilities, correlationId, locale, cancellationToken).ConfigureAwait(false);
        return engine.EvaluateBatch(resolved);
    }
}

public interface ICapabilityActionAuthorizer
{
    Task<CapabilityDecisionResult> RequireAllowedAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default);
}

public sealed class CapabilityActionAuthorizer(ICapabilityDecisionService decisions) : ICapabilityActionAuthorizer
{
    public async Task<CapabilityDecisionResult> RequireAllowedAsync(
        CapabilityId capability,
        string correlationId,
        string locale,
        CancellationToken cancellationToken = default)
    {
        // Intentionally reevaluate every invocation. State-changing endpoints call this immediately
        // before their mutation and must never authorize from a browser preview or cached session result.
        var decision = await decisions.EvaluateAsync(capability, correlationId, locale, cancellationToken).ConfigureAwait(false);
        return decision.IsAllowed ? decision : throw new CapabilityDecisionDeniedException(decision);
    }
}

public sealed class CapabilityDecisionDeniedException(CapabilityDecisionResult decision)
    : InvalidOperationException($"Capability '{decision.Capability}' was blocked: {decision.ReasonCode}.")
{
    public CapabilityDecisionResult Decision { get; } = decision;
}
