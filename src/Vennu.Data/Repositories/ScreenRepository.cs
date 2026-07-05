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

    public async Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Screen> screens = (await dataAccess.QueryAsync<Screen, object>(new { VenueId = venueId }, cancellationToken).ConfigureAwait(false)).ToArray();
        return screens;
    }

    public async Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        var screen = await dataAccess.QueryAsync<Screen>(new { Id = screenId }, cancellationToken).ConfigureAwait(false);

        if (screen is null)
        {
            return false;
        }

        screen.LastSeen = lastSeenUtc;
        screen.Status = status;
        screen.UpdatedUtc = DateTime.UtcNow;
        return await dataAccess.UpdateAsync(screen, cancellationToken).ConfigureAwait(false) > 0;
    }
}