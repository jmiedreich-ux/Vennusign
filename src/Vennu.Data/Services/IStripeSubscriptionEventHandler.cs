namespace Vennu.Data.Services;

public interface IStripeSubscriptionEventHandler
{
    Task<bool> HandleAsync(
        StripeSubscriptionEvent stripeEvent,
        CancellationToken cancellationToken = default);
}
