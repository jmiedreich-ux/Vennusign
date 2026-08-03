using Vennu.DataAccess;
using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public class ScreenRepository : IScreenRepository
{
    private readonly ISqlDataAccess dataAccess;

    public ScreenRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<Guid> CreateAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(screen);

        if (screen.Id == Guid.Empty)
        {
            screen.Id = Guid.NewGuid();
        }

        var utcNow = DateTime.UtcNow;
        screen.CreatedUtc = screen.CreatedUtc == default ? utcNow : screen.CreatedUtc;
        screen.UpdatedUtc = screen.UpdatedUtc == default ? utcNow : screen.UpdatedUtc;

        await dataAccess.InsertAsync(screen, cancellationToken).ConfigureAwait(false);
        return screen.Id;
    }

    public async Task<bool> AssignVenueAsync(Guid screenId, Guid venueId, CancellationToken cancellationToken = default)
    {
        var screen = await dataAccess.QueryAsync<Screen>(new { Id = screenId }, cancellationToken).ConfigureAwait(false);

        if (screen is null)
        {
            return false;
        }

        screen.VenueId = venueId;
        screen.UpdatedUtc = DateTime.UtcNow;
        return await dataAccess.UpdateAsync(screen, cancellationToken).ConfigureAwait(false) > 0;
    }

    public Task<Screen?> GetByIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        return dataAccess.QueryAsync<Screen>(new { Id = screenId }, cancellationToken);
    }

    public Task<Screen?> GetByScreenKeyAsync(string screenKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(screenKey);
        return dataAccess.QueryAsync<Screen>(new { ScreenKey = screenKey }, cancellationToken);
    }

    public Task<Screen?> GetByPreRegistrationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        return dataAccess.QueryAsync<Screen>(new { PreRegistrationTokenHash = tokenHash }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Screen>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await dataAccess.QueryAllAsync<Screen>(cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Screen> screens = (await dataAccess.QueryAsync<Screen, object>(new { VenueId = venueId }, cancellationToken).ConfigureAwait(false)).ToArray();
        return screens;
    }

    public async Task<bool> UpdateAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(screen);
        screen.UpdatedUtc = DateTime.UtcNow;
        return await dataAccess.UpdateAsync(screen, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<bool> ClaimPreRegisteredAsync(
        Guid screenId,
        string platform,
        string appVersion,
        DateTime claimedUtc,
        CancellationToken cancellationToken = default)
    {
        var screen = await dataAccess.QueryAsync<Screen>(new { Id = screenId }, cancellationToken).ConfigureAwait(false);
        if (screen is null || string.IsNullOrWhiteSpace(screen.PreRegistrationTokenHash))
        {
            return false;
        }

        screen.Platform = platform;
        screen.AppVersion = appVersion;
        screen.PreRegistrationTokenHash = null;
        screen.PreRegistrationExpiresUtc = null;
        screen.PreRegisteredUtc = claimedUtc;
        screen.UpdatedUtc = claimedUtc;
        return await dataAccess.UpdateAsync(screen, cancellationToken).ConfigureAwait(false) > 0;
    }

    public Task<bool> UpdateHeartbeatAsync(
        Guid screenId,
        DateTime lastSeenUtc,
        string status,
        CancellationToken cancellationToken = default) =>
        UpdateHeartbeatAsync(screenId, lastSeenUtc, status, null, null, cancellationToken);

    public async Task<bool> UpdateHeartbeatAsync(
        Guid screenId,
        DateTime lastSeenUtc,
        string status,
        string? platform,
        string? appVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        var screen = await dataAccess.QueryAsync<Screen>(new { Id = screenId }, cancellationToken).ConfigureAwait(false);

        if (screen is null)
        {
            return false;
        }

        if (string.Equals(screen.Status, "Archived", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        screen.LastSeen = lastSeenUtc;
        screen.Status = status;
        if (!string.IsNullOrWhiteSpace(platform))
        {
            screen.Platform = platform;
        }
        if (!string.IsNullOrWhiteSpace(appVersion))
        {
            screen.AppVersion = appVersion;
        }
        screen.UpdatedUtc = DateTime.UtcNow;
        return await dataAccess.UpdateAsync(screen, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<int> MarkStaleOnlineScreensOfflineAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        var screens = await dataAccess.QueryAllAsync<Screen>(cancellationToken).ConfigureAwait(false);
        var staleScreens = screens
            .Where(screen => string.Equals(screen.Status, "Online", StringComparison.OrdinalIgnoreCase)
                && (!screen.LastSeen.HasValue || screen.LastSeen.Value < cutoffUtc))
            .ToArray();

        if (staleScreens.Length == 0)
        {
            return 0;
        }

        var updatedUtc = DateTime.UtcNow;
        foreach (var screen in staleScreens)
        {
            screen.Status = "Offline";
            screen.UpdatedUtc = updatedUtc;
        }

        await dataAccess.UpdateAllAsync(staleScreens, cancellationToken: cancellationToken).ConfigureAwait(false);
        return staleScreens.Length;
    }
}
