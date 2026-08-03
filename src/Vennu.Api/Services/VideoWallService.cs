using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Services;

public sealed class VideoWallService(
    IScreenRepository screenRepository,
    IVenueRepository venueRepository,
    IFeatureResolutionService featureResolution,
    IScreenUpdateNotifier notifier) : IVideoWallService
{
    private static readonly IReadOnlyDictionary<string, int> LayoutSizes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["2x1"] = 2,
            ["3x1"] = 3,
            ["2x2"] = 4
        };

    public async Task<VideoWallSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var screens = await screenRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var groups = screens
            .Where(screen => !IsArchived(screen) && !string.IsNullOrWhiteSpace(screen.WallGroup) && screen.WallPosition.HasValue)
            .GroupBy(screen => screen.WallGroup!, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var members = group
                    .OrderBy(screen => screen.WallPosition)
                    .ThenBy(screen => screen.Id)
                    .Select(screen => new VideoWallScreen(screen.Id, screen.Name, screen.WallPosition!.Value))
                    .ToArray();
                return new VideoWallGroup(group.Key, LayoutForCount(members.Length), members);
            })
            .ToArray();
        var enabled = await featureResolution.HasFeatureAsync(venueId, "video_wall", cancellationToken).ConfigureAwait(false);
        return new VideoWallSnapshot(enabled, groups);
    }

    public async Task<VideoWallGroup> SaveAsync(
        Guid venueId,
        string name,
        string layout,
        IReadOnlyCollection<Guid> screenIds,
        CancellationToken cancellationToken = default)
    {
        await RequireFeatureAsync(venueId, cancellationToken).ConfigureAwait(false);
        var normalizedName = NormalizeName(name);
        if (!LayoutSizes.TryGetValue(layout.Trim(), out var requiredScreens))
        {
            throw new ArgumentException("Layout must be 2x1, 3x1, or 2x2.", nameof(layout));
        }
        if (screenIds is null || screenIds.Count != requiredScreens || screenIds.Distinct().Count() != requiredScreens)
        {
            throw new ArgumentException($"Layout {layout} requires {requiredScreens} unique screens.", nameof(screenIds));
        }

        var screens = await screenRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var byId = screens.Where(screen => !IsArchived(screen)).ToDictionary(screen => screen.Id);
        if (screenIds.Any(screenId => !byId.ContainsKey(screenId)))
        {
            throw new ArgumentException("Every selected screen must belong to the venue.", nameof(screenIds));
        }

        var displacedGroups = screenIds
            .Select(screenId => byId[screenId].WallGroup)
            .Where(group => !string.IsNullOrWhiteSpace(group))
            .Append(normalizedName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var positions = screenIds.Select((screenId, index) => (screenId, position: index + 1)).ToDictionary(item => item.screenId, item => item.position);

        foreach (var screen in screens)
        {
            var shouldClear = screen.WallGroup is not null && displacedGroups.Contains(screen.WallGroup);
            var selected = positions.TryGetValue(screen.Id, out var position);
            if (!shouldClear && !selected)
            {
                continue;
            }
            screen.WallGroup = selected ? normalizedName : null;
            screen.WallPosition = selected ? position : null;
            await screenRepository.UpdateAsync(screen, cancellationToken).ConfigureAwait(false);
        }

        await notifier.NotifyVenueContentUpdatedAsync(
            venueId,
            new { change = "video-wall-updated", wallGroup = normalizedName, layout },
            cancellationToken).ConfigureAwait(false);
        return new VideoWallGroup(
            normalizedName,
            layout.ToLowerInvariant(),
            screenIds.Select((screenId, index) => new VideoWallScreen(screenId, byId[screenId].Name, index + 1)).ToArray());
    }

    public async Task<bool> RemoveAsync(Guid venueId, string name, CancellationToken cancellationToken = default)
    {
        await RequireFeatureAsync(venueId, cancellationToken).ConfigureAwait(false);
        var normalizedName = NormalizeName(name);
        var screens = await screenRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var members = screens.Where(screen => string.Equals(screen.WallGroup, normalizedName, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (members.Length == 0)
        {
            return false;
        }
        foreach (var screen in members)
        {
            screen.WallGroup = null;
            screen.WallPosition = null;
            await screenRepository.UpdateAsync(screen, cancellationToken).ConfigureAwait(false);
        }
        await notifier.NotifyVenueContentUpdatedAsync(
            venueId,
            new { change = "video-wall-removed", wallGroup = normalizedName },
            cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task RequireVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        if (await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false) is null)
        {
            throw new KeyNotFoundException("Venue does not exist.");
        }
    }

    private async Task RequireFeatureAsync(Guid venueId, CancellationToken cancellationToken)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (!await featureResolution.HasFeatureAsync(venueId, "video_wall", cancellationToken).ConfigureAwait(false))
        {
            throw new ArgumentException("Video Wall requires the corresponding venue feature.");
        }
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Wall group name is required.", nameof(name));
        }
        var normalized = name.Trim();
        return normalized.Length <= 100
            ? normalized
            : throw new ArgumentException("Wall group name cannot exceed 100 characters.", nameof(name));
    }

    private static string LayoutForCount(int count) => count switch
    {
        2 => "2x1",
        3 => "3x1",
        4 => "2x2",
        _ => $"{count}-screen"
    };

    private static bool IsArchived(Screen screen) =>
        string.Equals(screen.Status, "Archived", StringComparison.OrdinalIgnoreCase);
}
