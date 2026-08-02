namespace Vennu.Data.Repositories;

public interface ISuperAdminOnboardingRepository
{
    Task<IReadOnlyCollection<SuperAdminOnboardingRecord>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default);
}

public sealed class SuperAdminOnboardingRecord
{
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public Guid? VenueId { get; set; }
    public string? VenueName { get; set; }
    public Guid? TierId { get; set; }
    public string? TierName { get; set; }
    public string SubscriptionStatus { get; set; } = "not-selected";
    public DateTime? TrialEndsAt { get; set; }
    public Guid? FirstScreenId { get; set; }
    public string? FirstScreenName { get; set; }
    public string FirstScreenStatus { get; set; } = "not-paired";
    public DateTime? FirstScreenLastSeenUtc { get; set; }
    public DateTime LastActivityUtc { get; set; }
}
