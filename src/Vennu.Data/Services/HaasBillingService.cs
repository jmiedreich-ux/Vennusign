using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class HaasBillingService(
    IVenueSubscriptionRepository subscriptionRepository,
    IHaasContractRepository contractRepository,
    IStripeHaasCheckoutSessionGateway gateway,
    TimeProvider timeProvider) : IHaasBillingService
{
    public async Task<HaasBillingPresentation> GetPresentationAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        RequireVenue(venueId);
        var contract = await contractRepository.GetCurrentByVenueIdAsync(venueId, cancellationToken)
            .ConfigureAwait(false);
        return new HaasBillingPresentation(
            HaasBundleCatalog.All,
            contract is null ? null : ToDisclosure(contract, timeProvider.GetUtcNow().UtcDateTime));
    }

    public async Task<StripeHaasCheckoutSessionResult> CreateCheckoutAsync(
        Guid venueId,
        string bundleKey,
        int termMonths,
        CancellationToken cancellationToken = default)
    {
        RequireVenue(venueId);
        var bundle = HaasBundleCatalog.GetRequired(bundleKey, termMonths);
        _ = await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Venue '{venueId}' does not have a software subscription.");
        var current = await contractRepository.GetCurrentByVenueIdAsync(venueId, cancellationToken)
            .ConfigureAwait(false);
        if (current is not null && current.Status is "active" or "past_due")
        {
            throw new InvalidOperationException("The venue already has a current HaaS contract.");
        }

        return await gateway.CreateAsync(
            new StripeHaasCheckoutSessionRequest(
                venueId,
                bundle.Key,
                bundle.TermMonths,
                bundle.MonthlyAmount),
            cancellationToken).ConfigureAwait(false);
    }

    internal static HaasContractDisclosure ToDisclosure(
        Vennu.Core.Models.HaasContract contract,
        DateTime utcNow)
    {
        var bundle = HaasBundleCatalog.GetRequired(contract.BundleKey, contract.TermMonths);
        var remainingMonths = RemainingMonths(contract.StartedUtc, contract.TermMonths, utcNow);
        return new HaasContractDisclosure(
            contract.BundleKey,
            bundle.Name,
            contract.Status,
            contract.TermMonths,
            contract.MonthlyAmount,
            contract.StartedUtc,
            contract.ContractEndsUtc,
            remainingMonths,
            decimal.Round(remainingMonths * contract.MonthlyAmount, 2, MidpointRounding.AwayFromZero),
            contract.CancelAtPeriodEnd,
            contract.EndedUtc);
    }

    internal static int RemainingMonths(DateTime startedUtc, int termMonths, DateTime utcNow)
    {
        var normalizedNow = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();
        var remaining = 0;
        for (var installment = 1; installment <= termMonths; installment++)
        {
            if (startedUtc.AddMonths(installment) > normalizedNow)
            {
                remaining++;
            }
        }

        return remaining;
    }

    private static void RequireVenue(Guid venueId)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }
    }
}
