using System.Net;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed record ToastPollingRunResult(bool OverlapSkipped, int ConnectionsDue, int Succeeded, int Failed);

public sealed record ToastPollingConnectionResult(bool Succeeded, string? ErrorCode = null);

public interface IToastPollingCoordinator
{
    Task<IReadOnlyCollection<Guid>> GetDueVenueIdsAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<ToastPollingConnectionResult> PollAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);
}

public sealed class ToastPollingCoordinator(
    IServiceScopeFactory scopeFactory,
    IOptions<ToastPollingOptions> options,
    TimeProvider timeProvider,
    ILogger<ToastPollingCoordinator> logger) : IToastPollingCoordinator
{
    public async Task<IReadOnlyCollection<Guid>> GetDueVenueIdsAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPosConnectionRepository>();
        return (await repository.GetAllByProviderAsync(PosProvider.Toast, cancellationToken).ConfigureAwait(false))
            .Where(value => value.Status == PosConnectionStatus.Connected &&
                            (!value.NextSyncAttemptUtc.HasValue || value.NextSyncAttemptUtc <= utcNow))
            .Select(value => value.VenueId)
            .Distinct()
            .OrderBy(value => value)
            .ToArray();
    }

    public async Task<ToastPollingConnectionResult> PollAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var repository = services.GetRequiredService<IPosConnectionRepository>();
        var connection = await repository.GetAsync(venueId, PosProvider.Toast, cancellationToken).ConfigureAwait(false);
        if (connection is null || connection.Status != PosConnectionStatus.Connected)
            return new ToastPollingConnectionResult(true);
        var attemptedUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            var mappings = await services.GetRequiredService<IPosCatalogMappingRepository>()
                .GetAllAsync(venueId, PosProvider.Toast, cancellationToken).ConfigureAwait(false);
            var itemIds = mappings
                .Where(value => value.EntityType == PosCatalogEntityType.Item)
                .Select(value => value.ExternalId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var provider = services.GetServices<IPosProvider>().Single(value => value.Provider == PosProvider.Toast);
            var accessToken = services.GetRequiredService<IPosCredentialProtector>()
                .Unprotect(connection.ProtectedAccessToken);
            var inventory = await provider.GetInventoryAsync(
                new PosProviderContext(venueId, connection.ExternalMerchantId, accessToken, itemIds),
                cancellationToken).ConfigureAwait(false);
            var applied = await services.GetRequiredService<IToastInventorySyncService>()
                .ApplySnapshotAsync(venueId, inventory.Items, cancellationToken).ConfigureAwait(false);

            var completedUtc = timeProvider.GetUtcNow().UtcDateTime;
            connection.LastSyncAttemptUtc = attemptedUtc;
            connection.LastSyncedUtc = completedUtc;
            connection.ConsecutiveSyncFailures = 0;
            connection.NextSyncAttemptUtc = completedUtc + PollInterval();
            connection.LastSyncErrorCode = null;
            await repository.SaveAsync(venueId, connection, cancellationToken).ConfigureAwait(false);
            logger.LogInformation(
                "Toast stock polling completed for venue {VenueId}; {ItemsUpdated} item(s) changed.",
                venueId, applied.ItemsUpdated);
            return new ToastPollingConnectionResult(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var errorCode = ErrorCode(exception);
            connection.LastSyncAttemptUtc = attemptedUtc;
            connection.ConsecutiveSyncFailures = Math.Min(connection.ConsecutiveSyncFailures + 1, 30);
            connection.NextSyncAttemptUtc = attemptedUtc + Backoff(connection.ConsecutiveSyncFailures);
            connection.LastSyncErrorCode = errorCode;
            if (exception is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden })
                connection.Status = PosConnectionStatus.ReauthorizationRequired;
            await repository.SaveAsync(venueId, connection, cancellationToken).ConfigureAwait(false);
            logger.LogWarning(
                "Toast stock polling failed for venue {VenueId} with {ErrorCode}; retry is scheduled.",
                venueId, errorCode);
            return new ToastPollingConnectionResult(false, errorCode);
        }
    }

    internal TimeSpan Backoff(int failures)
    {
        var initial = options.Value.InitialFailureBackoff > TimeSpan.Zero
            ? options.Value.InitialFailureBackoff
            : TimeSpan.FromMinutes(5);
        var maximum = options.Value.MaximumFailureBackoff >= initial
            ? options.Value.MaximumFailureBackoff
            : initial;
        var multiplier = Math.Pow(2, Math.Clamp(failures - 1, 0, 20));
        return TimeSpan.FromTicks(Math.Min(maximum.Ticks, checked((long)Math.Min(long.MaxValue, initial.Ticks * multiplier))));
    }

    private TimeSpan PollInterval() =>
        options.Value.PollInterval > TimeSpan.Zero ? options.Value.PollInterval : TimeSpan.FromHours(1);

    private static string ErrorCode(Exception exception) => exception switch
    {
        HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden } => "toast_authorization_failed",
        HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } => "toast_rate_limited",
        HttpRequestException => "toast_provider_unavailable",
        InvalidDataException => "toast_snapshot_incomplete",
        _ => "toast_poll_failed"
    };
}

public sealed class ToastPollingService(
    IToastPollingCoordinator coordinator,
    IOptions<ToastPollingOptions> options,
    TimeProvider timeProvider,
    ILogger<ToastPollingService> logger) : BackgroundService
{
    private readonly SemaphoreSlim cycleGate = new(1, 1);

    public async Task<ToastPollingRunResult> CheckOnceAsync(CancellationToken cancellationToken = default)
    {
        if (!await cycleGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return new ToastPollingRunResult(true, 0, 0, 0);
        try
        {
            var due = await coordinator.GetDueVenueIdsAsync(
                timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
            var succeeded = 0;
            var failed = 0;
            for (var index = 0; index < due.Count; index++)
            {
                if (index > 0 && options.Value.InterConnectionDelay > TimeSpan.Zero)
                    await Task.Delay(options.Value.InterConnectionDelay, timeProvider, cancellationToken).ConfigureAwait(false);
                try
                {
                    var result = await coordinator.PollAsync(due.ElementAt(index), cancellationToken).ConfigureAwait(false);
                    if (result.Succeeded) succeeded++; else failed++;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    failed++;
                    logger.LogError("Toast polling state could not be persisted for venue {VenueId}; continuing the cycle.", due.ElementAt(index));
                }
            }
            return new ToastPollingRunResult(false, due.Count, succeeded, failed);
        }
        finally
        {
            cycleGate.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = options.Value.PollInterval > TimeSpan.Zero
            ? options.Value.PollInterval
            : TimeSpan.FromHours(1);
        using var timer = new PeriodicTimer(interval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            await CheckOnceAsync(stoppingToken).ConfigureAwait(false);
    }
}
