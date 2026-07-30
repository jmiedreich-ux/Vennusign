using Vennu.Api.Contracts.Admin;
using Vennu.Api.Infrastructure;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Services;

public sealed class ScreenManagementService(
    IScreenRepository screenRepository,
    IVenueRepository venueRepository,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : IScreenManagementService
{
    public async Task<IReadOnlyCollection<ScreenManagementItem>> GetAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var screens = await screenRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        return screens
            .OrderBy(screen => screen.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(screen => screen.Id)
            .Select(ToItem)
            .ToArray();
    }

    public async Task<ScreenManagementItem> CreateAsync(
        Guid venueId,
        string name,
        string? location,
        CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var screen = new Screen
        {
            VenueId = venueId,
            ScreenKey = await GenerateUniqueScreenKeyAsync(cancellationToken).ConfigureAwait(false),
            Name = NormalizeRequired(name, nameof(name)),
            Location = NormalizeOptional(location, nameof(location)),
            PhotoGridDensity = PhotoGridDensity.Default,
            DisplayLayout = ScreenLayout.Default,
            SplitRatio = ScreenSplitRatio.Default,
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
        CancellationToken cancellationToken = default)
    {
        var screen = await GetOwnedScreenAsync(venueId, screenId, cancellationToken).ConfigureAwait(false);
        if (screen is null)
        {
            return null;
        }

        screen.Name = NormalizeRequired(name, nameof(name));
        screen.Location = NormalizeOptional(location, nameof(location));
        screen.PhotoGridDensity = PhotoGridDensity.Normalize(photoGridDensity ?? screen.PhotoGridDensity);
        screen.DisplayLayout = ScreenLayout.Normalize(displayLayout ?? screen.DisplayLayout);
        screen.SplitRatio = ScreenSplitRatio.Normalize(splitRatio ?? screen.SplitRatio);
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
        if (screen is null)
        {
            return false;
        }

        await notifier.NotifyScreenContentUpdatedAsync(
            screen.Id,
            new { change = "manual-push", requestedUtc = timeProvider.GetUtcNow().UtcDateTime },
            cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<Venue> RequireVenueAsync(Guid venueId, CancellationToken cancellationToken) =>
        await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Venue does not exist.");

    private async Task<Screen?> GetOwnedScreenAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken)
    {
        var screen = await screenRepository.GetByIdAsync(screenId, cancellationToken).ConfigureAwait(false);
        return screen?.VenueId == venueId ? screen : null;
    }

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

    private static ScreenManagementItem ToItem(Screen screen) =>
        new(
            screen.Id,
            screen.Name,
            screen.Location,
            PhotoGridDensity.Normalize(screen.PhotoGridDensity),
            ScreenLayout.Normalize(screen.DisplayLayout),
            ScreenSplitRatio.Normalize(screen.SplitRatio),
            screen.Status,
            screen.LastSeen,
            $"/display/{screen.Id}");
}
