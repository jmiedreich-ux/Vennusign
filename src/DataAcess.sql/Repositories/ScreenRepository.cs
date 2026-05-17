#nullable enable
using RepoDb;
using Vennu.Data.Models;
using Vennu.Data.Repositories;
using Vennu.DataAccess.Infrastructure;

namespace Vennu.DataAccess.Repositories;

public class ScreenRepository : IScreenRepository
{
    private readonly ISqlConnectionFactory connectionFactory;

    public ScreenRepository(ISqlConnectionFactory connectionFactory) => this.connectionFactory = connectionFactory;

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

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await connection.InsertAsync(screen);
        return screen.Id;
    }

    public async Task<bool> AssignVenueAsync(Guid screenId, Guid venueId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var screens = await connection.QueryAsync<Screen>(new { Id = screenId });
        var screen = screens.FirstOrDefault();

        if (screen is null)
        {
            return false;
        }

        screen.VenueId = venueId;
        screen.UpdatedUtc = DateTime.UtcNow;
        return await connection.UpdateAsync(screen) > 0;
    }

    public async Task<Screen?> GetByIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var screens = await connection.QueryAsync<Screen>(new { Id = screenId });
        return screens.FirstOrDefault();
    }

    public async Task<Screen?> GetByScreenKeyAsync(string screenKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(screenKey);

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var screens = await connection.QueryAsync<Screen>(new { ScreenKey = screenKey });
        return screens.FirstOrDefault();
    }

    public async Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var screens = await connection.QueryAsync<Screen>(new { VenueId = venueId });
        return screens.ToArray();
    }

    public async Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var screens = await connection.QueryAsync<Screen>(new { Id = screenId });
        var screen = screens.FirstOrDefault();

        if (screen is null)
        {
            return false;
        }

        screen.LastSeen = lastSeenUtc;
        screen.Status = status;
        screen.UpdatedUtc = DateTime.UtcNow;
        return await connection.UpdateAsync(screen) > 0;
    }
}
