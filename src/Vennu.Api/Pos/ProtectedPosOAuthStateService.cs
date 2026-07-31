using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;

namespace Vennu.Api.Pos;

public sealed class ProtectedPosOAuthStateService(
    IDataProtectionProvider dataProtectionProvider,
    IMemoryCache cache,
    TimeProvider timeProvider) : IPosOAuthStateService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector("Vennu.PosOAuthState.v1");

    public string Create(Guid venueId)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Venue ID is required.", nameof(venueId));
        var nonce = Guid.NewGuid().ToString("N");
        cache.Set(CacheKey(nonce), true, timeProvider.GetUtcNow().Add(Lifetime));
        return protector.Protect($"{venueId:N}|{nonce}|{timeProvider.GetUtcNow().UtcTicks}");
    }

    public Guid Consume(string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        string value;
        try { value = protector.Unprotect(state); }
        catch (Exception exception) when (exception is not ArgumentException)
        { throw new InvalidOperationException("The OAuth state is invalid.", exception); }

        var parts = value.Split('|');
        if (parts.Length != 3 || !Guid.TryParseExact(parts[0], "N", out var venueId) ||
            !long.TryParse(parts[2], out var issuedTicks) ||
            timeProvider.GetUtcNow() - new DateTimeOffset(issuedTicks, TimeSpan.Zero) > Lifetime ||
            !cache.TryGetValue(CacheKey(parts[1]), out _))
        {
            throw new InvalidOperationException("The OAuth state is invalid or expired.");
        }

        cache.Remove(CacheKey(parts[1]));
        return venueId;
    }

    private static string CacheKey(string nonce) => $"pos-oauth:{nonce}";
}
