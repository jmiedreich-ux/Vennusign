using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class HaasContractSubscriptionEventHandler(
    IStripeEventIdempotencyService idempotencyService,
    IHaasContractRepository repository,
    TimeProvider timeProvider) : IHaasContractSubscriptionEventHandler
{
    private const string Created = "customer.subscription.created";
    private const string Updated = "customer.subscription.updated";
    private const string Deleted = "customer.subscription.deleted";

    public Task<bool> HandleAsync(
        HaasContractSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stripeEvent);
        var eventType = Required(stripeEvent.EventType, nameof(stripeEvent.EventType));
        if (eventType is not (Created or Updated or Deleted))
        {
            throw new ArgumentOutOfRangeException(nameof(stripeEvent), stripeEvent.EventType, "Unsupported HaaS event type.");
        }

        return idempotencyService.ExecuteOnceAsync(
            Required(stripeEvent.EventId, nameof(stripeEvent.EventId)),
            eventType,
            token => ApplyAsync(stripeEvent with { EventType = eventType }, token),
            cancellationToken);
    }

    private async Task ApplyAsync(HaasContractSubscriptionEvent stripeEvent, CancellationToken cancellationToken)
    {
        var stripeId = Required(stripeEvent.StripeSubscriptionId, nameof(stripeEvent.StripeSubscriptionId));
        var existing = await repository.GetByStripeSubscriptionIdAsync(stripeId, cancellationToken)
            .ConfigureAwait(false);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (stripeEvent.EventType == Deleted)
        {
            var deleted = existing
                ?? throw new KeyNotFoundException($"HaaS subscription '{stripeId}' is not assigned to a contract.");
            deleted.Status = "canceled";
            deleted.CancelAtPeriodEnd = false;
            deleted.EndedUtc = utcNow;
            deleted.UpdatedUtc = utcNow;
            await SaveAsync(deleted, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (stripeEvent.VenueId is null || stripeEvent.VenueId == Guid.Empty ||
            stripeEvent.TermMonths is null || stripeEvent.StartedUtc is null)
        {
            throw new ArgumentException("Confirmed HaaS subscription events require venue, term, and start metadata.", nameof(stripeEvent));
        }

        var bundle = HaasBundleCatalog.GetRequired(
            Required(stripeEvent.BundleKey, nameof(stripeEvent.BundleKey)),
            stripeEvent.TermMonths.Value);
        var status = NormalizeStatus(stripeEvent.Status);
        if (existing is not null && existing.VenueId != stripeEvent.VenueId.Value)
        {
            throw new InvalidOperationException("The HaaS subscription is already assigned to another venue.");
        }

        var contract = existing ?? new HaasContract
        {
            Id = Guid.NewGuid(),
            VenueId = stripeEvent.VenueId.Value,
            StripeSubscriptionId = stripeId,
            CreatedUtc = utcNow
        };
        contract.BundleKey = bundle.Key;
        contract.TermMonths = bundle.TermMonths;
        contract.MonthlyAmount = bundle.MonthlyAmount;
        contract.Status = status;
        contract.StartedUtc = stripeEvent.StartedUtc.Value.ToUniversalTime();
        contract.ContractEndsUtc = contract.StartedUtc.AddMonths(bundle.TermMonths);
        contract.CancelAtPeriodEnd = stripeEvent.CancelAtPeriodEnd;
        contract.EndedUtc = null;
        contract.UpdatedUtc = utcNow;
        await SaveAsync(contract, cancellationToken).ConfigureAwait(false);
    }

    private async Task SaveAsync(HaasContract contract, CancellationToken cancellationToken)
    {
        if (!await repository.SaveAsync(contract, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The HaaS contract could not be persisted.");
        }
    }

    private static string NormalizeStatus(string? status)
    {
        var normalized = Required(status, nameof(status)).ToLowerInvariant();
        return normalized switch
        {
            "active" => "active",
            "past_due" => "past_due",
            "canceled" => "canceled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported HaaS subscription status.")
        };
    }

    private static string Required(string? value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }
}
