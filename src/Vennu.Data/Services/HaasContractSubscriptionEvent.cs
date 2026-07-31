namespace Vennu.Data.Services;

public sealed record HaasContractSubscriptionEvent(
    string EventId,
    string EventType,
    string StripeSubscriptionId,
    Guid? VenueId = null,
    string? BundleKey = null,
    int? TermMonths = null,
    string? Status = null,
    DateTime? StartedUtc = null,
    bool CancelAtPeriodEnd = false);

public interface IHaasContractSubscriptionEventHandler
{
    Task<bool> HandleAsync(
        HaasContractSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken = default);
}
