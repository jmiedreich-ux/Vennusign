using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Infrastructure;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Services;

public sealed class ScreenManagementService(
    IScreenRepository screenRepository,
    IVenueRepository venueRepository,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider,
    IVenueEntitlementService? entitlementService = null,
    IScreenContentDeliveryService? deliveryService = null) : IScreenManagementService
{
    public async Task<IReadOnlyCollection<ScreenManagementItem>> GetAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var screens = await screenRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var deliveries = deliveryService is null
            ? new Dictionary<Guid, ScreenContentDelivery>()
            : await deliveryService.GetLatestByVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        return screens
            .OrderBy(screen => screen.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(screen => screen.Id)
            .Select(screen => ToItem(screen, deliveries.GetValueOrDefault(screen.Id)))
            .ToArray();
    }

    public async Task<ScreenManagementItem> CreateAsync(
        Guid venueId,
        string name,
        string? location,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (entitlementService is not null)
            await entitlementService.EnsureCanAddScreenAsync(venueId, cancellationToken).ConfigureAwait(false);
        var screen = new Screen
        {
            VenueId = venueId,
            ScreenKey = await GenerateUniqueScreenKeyAsync(cancellationToken).ConfigureAwait(false),
            Name = NormalizeRequired(name, nameof(name)),
            Location = NormalizeOptional(location, nameof(location)),
            PhotoGridDensity = PhotoGridDensity.Default,
            DisplayLayout = ScreenLayout.Default,
            SplitRatio = ScreenSplitRatio.Default,
            HeroDwellSeconds = HeroDwellSeconds.Default,
            Status = "Offline",
            CreatedUtc = timeProvider.GetUtcNow().UtcDateTime,
            UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime
        };
        screen.Id = await screenRepository.CreateAsync(screen, cancellationToken).ConfigureAwait(false);
        return ToItem(screen);
    }

    public async Task<ScreenManagementItem?> UpdateAsync(
        Guid venueId,
        Guid screenId,
        string name,
        string? location,
        string? photoGridDensity,
        string? displayLayout,
        string? splitRatio = null,
        int? heroDwellSeconds = null,
        CancellationToken cancellationToken = default)
    {
        var screen = await GetOwnedScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        if (screen is null || IsArchived(screen))
        {
            return null;
        }

        screen.Name = NormalizeRequired(name, nameof(name));
        screen.Location = NormalizeOptional(location, nameof(location));
        screen.PhotoGridDensity = PhotoGridDensity.Normalize(photoGridDensity ?? screen.PhotoGridDensity);
        screen.DisplayLayout = ScreenLayout.Normalize(displayLayout ?? screen.DisplayLayout);
        screen.SplitRatio = ScreenSplitRatio.Normalize(splitRatio ?? screen.SplitRatio);
        screen.HeroDwellSeconds = HeroDwellSeconds.Normalize(heroDwellSeconds ?? screen.HeroDwellSeconds);
        screen.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        return await screenRepository.UpdateAsync(screen, cancellationToken).ConfigureAwait(false)
            ? ToItem(screen)
            : null;
    }

    public async Task<bool> PushAsync(
        Guid venueId,
        Guid screenId,
        CancellationToken cancellationToken = default)
    {
        var screen = await GetOwnedScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        if (screen is null || IsArchived(screen))
        {
            return false;
        }

        var delivery = deliveryService is null
            ? null
            : await deliveryService.IssueAsync(venueId, screen.Id, cancellationToken).ConfigureAwait(false);
        if (deliveryService is not null && delivery is null) return false;
        await notifier.NotifyScreenContentUpdatedAsync(screen.Id, new
        {
            change = "manual-push",
            requestedUtc = delivery?.RequestedUtc ?? timeProvider.GetUtcNow().UtcDateTime,
            revision = delivery?.AuthoritativeRevision
        }, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<ScreenManagementItem?> SetArchivedAsync(
        Guid venueId,
        Guid screenId,
        bool archived,
        CancellationToken cancellationToken = default)
    {
        var screen = await GetOwnedScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        if (screen is null)
        {
            return null;
        }

        screen.Status = archived ? "Archived" : "Offline";
        if (archived)
        {
            screen.WallGroup = null;
            screen.WallPosition = null;
        }
        screen.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        return await screenRepository.UpdateAsync(screen, cancellationToken).ConfigureAwait(false)
            ? ToItem(screen)
            : null;
    }

    public async Task<ScreenManagementItem?> ResetAsync(
        Guid venueId,
        Guid screenId,
        CancellationToken cancellationToken = default)
    {
        var screen = await GetOwnedScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        if (screen is null || IsArchived(screen))
        {
            return null;
        }

        screen.Status = "Offline";
        screen.LastSeen = null;
        screen.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        return await screenRepository.UpdateAsync(screen, cancellationToken).ConfigureAwait(false)
            ? ToItem(screen)
            : null;
    }

    public async Task<bool> UnpairAsync(
        Guid venueId,
        Guid screenId,
        CancellationToken cancellationToken = default)
    {
        var screen = await GetOwnedScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        if (screen is null)
        {
            return false;
        }

        screen.VenueId = null;
        screen.Status = "Unpaired";
        screen.LastSeen = null;
        screen.WallGroup = null;
        screen.WallPosition = null;
        screen.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        return await screenRepository.UpdateAsync(screen, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Venue> RequireVenueAsync(Guid venueId, CancellationToken cancellationToken) =>
        await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Venue does not exist.");

    private async Task<Screen?> GetOwnedScreenAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken)
    {
        var screen = await screenRepository.GetByIdAsync(screenId, cancellationToken).ConfigureAwait(false);
        return screen?.VenueId == venueId ? screen : null;
    }

    private static bool IsArchived(Screen screen) =>
        string.Equals(screen.Status, "Archived", StringComparison.OrdinalIgnoreCase);

    private async Task<string> GenerateUniqueScreenKeyAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var screenKey = IdentifierGenerator.CreateScreenKey();
            if (await screenRepository.GetByScreenKeyAsync(screenKey, cancellationToken).ConfigureAwait(false) is null)
            {
                return screenKey;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique screen key.");
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Screen name is required.", parameterName);
        }
        var normalized = value.Trim();
        return normalized.Length <= 200
            ? normalized
            : throw new ArgumentException("Screen name cannot exceed 200 characters.", parameterName);
    }

    private static string? NormalizeOptional(string? value, string parameterName)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is null)
        {
            return null;
        }
        return normalized.Length <= 200
            ? normalized
            : throw new ArgumentException("Screen location cannot exceed 200 characters.", parameterName);
    }

    private static ScreenManagementItem ToItem(Screen screen, ScreenContentDelivery? delivery = null) =>
        new(
            screen.Id,
            screen.Name,
            screen.Location,
            PhotoGridDensity.Normalize(screen.PhotoGridDensity),
            ScreenLayout.Normalize(screen.DisplayLayout),
            ScreenSplitRatio.Normalize(screen.SplitRatio),
            HeroDwellSeconds.Normalize(screen.HeroDwellSeconds),
            screen.Status,
            screen.LastSeen,
            screen.Platform,
            screen.AppVersion,
            $"/display/{screen.Id}",
            delivery?.AuthoritativeRevision,
            delivery?.AppliedRevision,
            delivery?.State,
            delivery?.RequestedUtc,
            delivery?.AppliedUtc,
            delivery?.FailureCode,
            delivery?.FailureDetail);
}
