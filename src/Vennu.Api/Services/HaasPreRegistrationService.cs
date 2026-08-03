using System.Security.Cryptography;
using System.Text;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.Infrastructure;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Services;

public sealed class HaasPreRegistrationService(
    IScreenRepository screenRepository,
    IVenueRepository venueRepository,
    TimeProvider timeProvider) : IHaasPreRegistrationService
{
    public async Task<HaasPreRegistrationResponse> CreateAsync(
        Guid venueId,
        HaasPreRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _ = await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Venue does not exist.");

        var name = NormalizeRequired(request.Name, 200, "Screen name");
        var location = NormalizeOptional(request.Location, 200, "Screen location");
        var platform = ScreenPlatform.Normalize(request.Platform);
        var desiredVersion = NormalizeRequired(request.DesiredAppVersion, 50, "Desired app version");
        var deliveryReference = NormalizeRequired(request.DeliveryReference, 100, "Delivery reference");
        var expiresInHours = request.ExpiresInHours ?? 168;
        if (expiresInHours is < 1 or > 720)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Expiry must be between 1 and 720 hours.");
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresUtc = now.AddHours(expiresInHours);
        var screen = new Screen
        {
            VenueId = venueId,
            ScreenKey = await GenerateUniqueScreenKeyAsync(cancellationToken).ConfigureAwait(false),
            Name = name,
            Location = location,
            Platform = platform,
            DesiredAppVersion = desiredVersion,
            DeliveryReference = deliveryReference,
            PreRegistrationTokenHash = Hash(token),
            PreRegistrationExpiresUtc = expiresUtc,
            Status = "Offline",
            CreatedUtc = now,
            UpdatedUtc = now
        };
        screen.Id = await screenRepository.CreateAsync(screen, cancellationToken).ConfigureAwait(false);

        return new HaasPreRegistrationResponse(
            screen.Id,
            screen.ScreenKey,
            platform,
            desiredVersion,
            deliveryReference,
            expiresUtc,
            token,
            "/provision");
    }

    public async Task<ClaimPreRegisteredScreenResponse?> ClaimAsync(
        ClaimPreRegisteredScreenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = NormalizeRequired(request.Token, 128, "Bootstrap token");
        var platform = ScreenPlatform.Normalize(request.Platform);
        var appVersion = NormalizeRequired(request.AppVersion, 50, "App version");
        var screen = await screenRepository
            .GetByPreRegistrationTokenHashAsync(Hash(token), cancellationToken)
            .ConfigureAwait(false);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (screen?.VenueId is null ||
            screen.PreRegistrationExpiresUtc is null ||
            screen.PreRegistrationExpiresUtc <= now ||
            !string.Equals(screen.Platform, platform, StringComparison.Ordinal))
        {
            return null;
        }

        var claimed = await screenRepository
            .ClaimPreRegisteredAsync(screen.Id, platform, appVersion, now, cancellationToken)
            .ConfigureAwait(false);
        return claimed
            ? new ClaimPreRegisteredScreenResponse(
                screen.Id,
                screen.ScreenKey,
                screen.VenueId.Value,
                $"/display/{screen.Id}")
            : null;
    }

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private async Task<string> GenerateUniqueScreenKeyAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var key = IdentifierGenerator.CreateScreenKey();
            if (await screenRepository.GetByScreenKeyAsync(key, cancellationToken).ConfigureAwait(false) is null)
            {
                return key;
            }
        }
        throw new InvalidOperationException("Unable to generate a unique screen key.");
    }

    private static string NormalizeRequired(string? value, int maxLength, string label)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException($"{label} is required.");
        }
        return normalized.Length <= maxLength
            ? normalized
            : throw new ArgumentException($"{label} cannot exceed {maxLength} characters.");
    }

    private static string? NormalizeOptional(string? value, int maxLength, string label)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        return normalized is null || normalized.Length <= maxLength
            ? normalized
            : throw new ArgumentException($"{label} cannot exceed {maxLength} characters.");
    }
}
