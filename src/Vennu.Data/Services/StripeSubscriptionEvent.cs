namespace Vennu.Data.Services;

public sealed record StripeSubscriptionEvent(
    string EventId,
    string EventType,
    string StripeSubscriptionId,
    Guid? VenueId = null,
    string? StripePriceId = null,
    string? Status = null,
    DateTime? TrialEndsAt = null,
    DateTime? CurrentPeriodEnd = null,
    bool CancelAtPeriodEnd = false);
