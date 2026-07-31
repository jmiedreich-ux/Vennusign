namespace Vennu.Data.Services;

public sealed record HaasBundleDefinition(
    string Key,
    string Name,
    int TermMonths,
    decimal MonthlyAmount,
    string PostContractTierSlug);

public sealed record HaasContractDisclosure(
    string BundleKey,
    string BundleName,
    string Status,
    int TermMonths,
    decimal MonthlyAmount,
    DateTime StartedUtc,
    DateTime ContractEndsUtc,
    int RemainingMonths,
    decimal EstimatedBuyoutAmount,
    bool CancelAtPeriodEnd,
    DateTime? EndedUtc);

public sealed record HaasBillingPresentation(
    IReadOnlyCollection<HaasBundleDefinition> Bundles,
    HaasContractDisclosure? Contract);

public sealed record StripeHaasCheckoutSessionRequest(
    Guid VenueId,
    string BundleKey,
    int TermMonths,
    decimal MonthlyAmount);

public sealed record StripeHaasCheckoutSessionResult(Uri CheckoutUrl);

public interface IStripeHaasCheckoutSessionGateway
{
    Task<StripeHaasCheckoutSessionResult> CreateAsync(
        StripeHaasCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);
}

public interface IHaasBillingService
{
    Task<HaasBillingPresentation> GetPresentationAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<StripeHaasCheckoutSessionResult> CreateCheckoutAsync(
        Guid venueId,
        string bundleKey,
        int termMonths,
        CancellationToken cancellationToken = default);
}

public static class HaasBundleCatalog
{
    private static readonly HaasBundleDefinition[] Definitions =
    [
        new("starter_kit", "Starter Kit", 18, 89m, "starter"),
        new("bar_pack", "Bar Pack", 24, 159m, "pro"),
        new("full_house", "Full House", 36, 249m, "business")
    ];

    public static IReadOnlyCollection<HaasBundleDefinition> All => Definitions;

    public static HaasBundleDefinition GetRequired(string bundleKey, int termMonths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bundleKey);
        return Definitions.SingleOrDefault(definition =>
            definition.Key.Equals(bundleKey.Trim(), StringComparison.OrdinalIgnoreCase) &&
            definition.TermMonths == termMonths)
            ?? throw new ArgumentException("The HaaS bundle and term combination is not eligible.", nameof(bundleKey));
    }
}
