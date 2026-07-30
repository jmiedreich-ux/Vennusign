using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class DateRangePromotionService(
    IDateRangePromotionRepository repository,
    IVenueRepository venues,
    IDateRangePromotionResolver resolver) : IDateRangePromotionService
{
    public Task<IReadOnlyCollection<DateRangePromotion>> GetAsync(
        Guid venueId, CancellationToken cancellationToken = default) =>
        repository.GetByVenueAsync(Require(venueId), cancellationToken);

    public async Task<DateRangePromotion?> GetActiveAsync(
        Guid venueId, DateTimeOffset utcNow, CancellationToken cancellationToken = default)
    {
        var venue = await venues.GetByIdAsync(Require(venueId), cancellationToken).ConfigureAwait(false);
        if (venue is null) return null;
        var rows = await repository.GetByVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        return resolver.Resolve(venue.Timezone, utcNow, rows).ActivePromotion;
    }

    public async Task<DateRangePromotion> CreateAsync(
        Guid venueId, DateRangePromotion promotion, DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);
        if (await venues.GetByIdAsync(Require(venueId), cancellationToken).ConfigureAwait(false) is null)
            throw new KeyNotFoundException("Venue was not found.");
        var now = utcNow.UtcDateTime;
        var value = Normalize(promotion, venueId, Guid.NewGuid(), now, now);
        await repository.CreateAsync(value, cancellationToken).ConfigureAwait(false);
        return value;
    }

    public async Task<DateRangePromotion?> UpdateAsync(
        Guid venueId, Guid promotionId, DateRangePromotion promotion, DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(promotion);
        var existing = (await repository.GetByVenueAsync(Require(venueId), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(item => item.Id == Require(promotionId));
        if (existing is null) return null;
        var value = Normalize(promotion, venueId, existing.Id, existing.CreatedUtc, utcNow.UtcDateTime);
        await repository.UpdateAsync(value, cancellationToken).ConfigureAwait(false);
        return value;
    }

    public async Task<DateRangePromotion?> ArchiveAsync(
        Guid venueId, Guid promotionId, DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        var existing = (await repository.GetByVenueAsync(Require(venueId), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(item => item.Id == Require(promotionId));
        if (existing is null) return null;
        existing.IsEnabled = false;
        existing.UpdatedUtc = utcNow.UtcDateTime;
        await repository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        return existing;
    }

    private static DateRangePromotion Normalize(
        DateRangePromotion source, Guid venueId, Guid id, DateTime createdUtc, DateTime updatedUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source.Name);
        if (source.Name.Trim().Length > 160) throw new ArgumentException("Name cannot exceed 160 characters.");
        if (source.EndLocalDate.Date < source.StartLocalDate.Date) throw new ArgumentException("End date cannot precede start date.");
        if (source.Priority is < -1000 or > 1000) throw new ArgumentOutOfRangeException(nameof(source.Priority));
        return new DateRangePromotion
        {
            Id = id, VenueId = venueId, Name = source.Name.Trim(),
            StartLocalDate = source.StartLocalDate.Date, EndLocalDate = source.EndLocalDate.Date,
            TargetLayout = NormalizeText(source.TargetLayout, 80),
            Title = NormalizeText(source.Title, 200), Body = NormalizeText(source.Body, 1000),
            Priority = source.Priority, IsEnabled = source.IsEnabled,
            CreatedUtc = createdUtc, UpdatedUtc = updatedUtc
        };
    }

    private static string? NormalizeText(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null
        : value.Trim().Length <= max ? value.Trim()
        : throw new ArgumentException($"Value cannot exceed {max} characters.");
    private static Guid Require(Guid id) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.") : id;
}
