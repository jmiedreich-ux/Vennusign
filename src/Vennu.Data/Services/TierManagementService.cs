using System.Text.RegularExpressions;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed partial class TierManagementService : ITierManagementService
{
    private readonly ISubscriptionTierRepository repository;
    private readonly TimeProvider timeProvider;

    public TierManagementService(ISubscriptionTierRepository repository, TimeProvider timeProvider)
    {
        this.repository = repository;
        this.timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .OrderBy(tier => tier.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(tier => tier.Id)
            .ToArray();

    public async Task<SubscriptionTier> CreateAsync(TierManagementRequest request, CancellationToken cancellationToken = default)
    {
        var tier = Build(Guid.NewGuid(), request, timeProvider.GetUtcNow().UtcDateTime, null);
        if (await repository.GetBySlugAsync(tier.Slug, cancellationToken).ConfigureAwait(false) is not null)
        {
            throw new InvalidOperationException("A tier with this slug already exists.");
        }

        if (!await repository.CreateAsync(tier, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The tier could not be created.");
        }

        return tier;
    }

    public async Task<SubscriptionTier?> UpdateAsync(Guid tierId, TierManagementRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false);
        if (existing is null) return null;
        var updated = Build(tierId, request, existing.CreatedUtc, timeProvider.GetUtcNow().UtcDateTime);
        var duplicate = await repository.GetBySlugAsync(updated.Slug, cancellationToken).ConfigureAwait(false);
        if (duplicate is not null && duplicate.Id != tierId)
        {
            throw new InvalidOperationException("A tier with this slug already exists.");
        }

        return await repository.UpdateAsync(updated, cancellationToken).ConfigureAwait(false) ? updated : null;
    }

    public async Task<SubscriptionTier?> CloneAsync(Guid tierId, CancellationToken cancellationToken = default)
    {
        var source = await repository.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false);
        if (source is null) return null;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var slug = await UniqueCloneSlugAsync(source.Slug, cancellationToken).ConfigureAwait(false);
        var clone = new SubscriptionTier
        {
            Id = Guid.NewGuid(), Name = $"{source.Name} Copy", Slug = slug, Price = source.Price,
            MaxScreens = source.MaxScreens, IsPublic = false, IsActive = false,
            CreatedUtc = utcNow, UpdatedUtc = utcNow
        };
        return await repository.CreateAsync(clone, cancellationToken).ConfigureAwait(false) ? clone : null;
    }

    public async Task<bool> ArchiveAsync(Guid tierId, CancellationToken cancellationToken = default)
    {
        var tier = await repository.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false);
        if (tier is null) return false;
        tier.IsActive = false;
        tier.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        return await repository.UpdateAsync(tier, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> UniqueCloneSlugAsync(string sourceSlug, CancellationToken cancellationToken)
    {
        for (var suffix = 1; suffix <= 100; suffix++)
        {
            var candidate = $"{sourceSlug}-copy{(suffix == 1 ? string.Empty : $"-{suffix}")}";
            if (await repository.GetBySlugAsync(candidate, cancellationToken).ConfigureAwait(false) is null) return candidate;
        }
        throw new InvalidOperationException("A unique clone slug could not be allocated.");
    }

    private static SubscriptionTier Build(Guid id, TierManagementRequest request, DateTime createdUtc, DateTime? updatedUtc)
    {
        ArgumentNullException.ThrowIfNull(request);
        var name = request.Name?.Trim() ?? string.Empty;
        var slug = SlugSeparators().Replace((request.Slug ?? string.Empty).Trim().ToLowerInvariant(), "-").Trim('-');
        if (name.Length == 0) throw new ArgumentException("Tier name is required.", nameof(request));
        if (slug.Length == 0) throw new ArgumentException("Tier slug is required.", nameof(request));
        if (request.Price < 0) throw new ArgumentOutOfRangeException(nameof(request), "Tier price cannot be negative.");
        if (request.MaxScreens != -1 && request.MaxScreens <= 0) throw new ArgumentOutOfRangeException(nameof(request), "Screen limit must be positive or -1.");
        return new SubscriptionTier
        {
            Id = id, Name = name, Slug = slug, Price = request.Price, MaxScreens = request.MaxScreens,
            IsPublic = request.IsPublic, IsActive = request.IsActive,
            StripeProductId = Clean(request.StripeProductId), StripeMonthlyPriceId = Clean(request.StripeMonthlyPriceId),
            StripeAnnualPriceId = Clean(request.StripeAnnualPriceId),
            CreatedUtc = createdUtc, UpdatedUtc = updatedUtc ?? createdUtc
        };
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugSeparators();
}
