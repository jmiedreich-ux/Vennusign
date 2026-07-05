using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.TestDoubles;

internal sealed class FakeVenueRepository : IVenueRepository
{
    public Func<Venue, CancellationToken, Task<Guid>>? CreateAsyncHandler { get; set; }
    public Func<Guid, CancellationToken, Task<Venue?>>? GetByIdAsyncHandler { get; set; }
    public Func<CancellationToken, Task<IReadOnlyCollection<Venue>>>? GetAllAsyncHandler { get; set; }
    public Venue? LastCreatedVenue { get; private set; }

    public Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        LastCreatedVenue = venue;

        if (CreateAsyncHandler is not null)
        {
            return CreateAsyncHandler(venue, cancellationToken);
        }

        venue.Id = venue.Id == Guid.Empty ? Guid.NewGuid() : venue.Id;
        return Task.FromResult(venue.Id);
    }

    public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (GetAllAsyncHandler is not null)
        {
            return GetAllAsyncHandler(cancellationToken);
        }

        return Task.FromResult<IReadOnlyCollection<Venue>>([]);
    }

    public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        if (GetByIdAsyncHandler is not null)
        {
            return GetByIdAsyncHandler(venueId, cancellationToken);
        }

        return Task.FromResult<Venue?>(null);
    }
}

internal sealed class FakeScreenRepository : IScreenRepository
{
    public Func<Screen, CancellationToken, Task<Guid>>? CreateAsyncHandler { get; set; }
    public Func<Guid, Guid, CancellationToken, Task<bool>>? AssignVenueAsyncHandler { get; set; }
    public Func<Guid, CancellationToken, Task<Screen?>>? GetByIdAsyncHandler { get; set; }
    public Func<string, CancellationToken, Task<Screen?>>? GetByScreenKeyAsyncHandler { get; set; }
    public Func<Guid, CancellationToken, Task<IReadOnlyCollection<Screen>>>? GetByVenueIdAsyncHandler { get; set; }
    public Func<Guid, DateTime, string, CancellationToken, Task<bool>>? UpdateHeartbeatAsyncHandler { get; set; }
    public Screen? LastCreatedScreen { get; private set; }

    public Task<Guid> CreateAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        LastCreatedScreen = screen;

        if (CreateAsyncHandler is not null)
        {
            return CreateAsyncHandler(screen, cancellationToken);
        }

        screen.Id = screen.Id == Guid.Empty ? Guid.NewGuid() : screen.Id;
        return Task.FromResult(screen.Id);
    }

    public Task<bool> AssignVenueAsync(Guid screenId, Guid venueId, CancellationToken cancellationToken = default)
    {
        if (AssignVenueAsyncHandler is not null)
        {
            return AssignVenueAsyncHandler(screenId, venueId, cancellationToken);
        }

        return Task.FromResult(true);
    }

    public Task<Screen?> GetByIdAsync(Guid screenId, CancellationToken cancellationToken = default)
    {
        if (GetByIdAsyncHandler is not null)
        {
            return GetByIdAsyncHandler(screenId, cancellationToken);
        }

        return Task.FromResult<Screen?>(null);
    }

    public Task<Screen?> GetByScreenKeyAsync(string screenKey, CancellationToken cancellationToken = default)
    {
        if (GetByScreenKeyAsyncHandler is not null)
        {
            return GetByScreenKeyAsyncHandler(screenKey, cancellationToken);
        }

        return Task.FromResult<Screen?>(null);
    }

    public Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        if (GetByVenueIdAsyncHandler is not null)
        {
            return GetByVenueIdAsyncHandler(venueId, cancellationToken);
        }

        return Task.FromResult<IReadOnlyCollection<Screen>>([]);
    }

    public Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, CancellationToken cancellationToken = default)
    {
        if (UpdateHeartbeatAsyncHandler is not null)
        {
            return UpdateHeartbeatAsyncHandler(screenId, lastSeenUtc, status, cancellationToken);
        }

        return Task.FromResult(true);
    }
}

internal sealed class FakeScreenPairingCodeRepository : IScreenPairingCodeRepository
{
    public Func<ScreenPairingCode, CancellationToken, Task<string>>? CreateAsyncHandler { get; set; }
    public Func<string, CancellationToken, Task<ScreenPairingCode?>>? GetByCodeAsyncHandler { get; set; }
    public Func<string, Guid, CancellationToken, Task<bool>>? ClaimAsyncHandler { get; set; }
    public ScreenPairingCode? LastCreatedPairingCode { get; private set; }

    public Task<string> CreateAsync(ScreenPairingCode pairingCode, CancellationToken cancellationToken = default)
    {
        LastCreatedPairingCode = pairingCode;

        if (CreateAsyncHandler is not null)
        {
            return CreateAsyncHandler(pairingCode, cancellationToken);
        }

        return Task.FromResult(pairingCode.Code);
    }

    public Task<ScreenPairingCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (GetByCodeAsyncHandler is not null)
        {
            return GetByCodeAsyncHandler(code, cancellationToken);
        }

        return Task.FromResult<ScreenPairingCode?>(null);
    }

    public Task<bool> ClaimAsync(string code, Guid venueId, CancellationToken cancellationToken = default)
    {
        if (ClaimAsyncHandler is not null)
        {
            return ClaimAsyncHandler(code, venueId, cancellationToken);
        }

        return Task.FromResult(true);
    }
}
