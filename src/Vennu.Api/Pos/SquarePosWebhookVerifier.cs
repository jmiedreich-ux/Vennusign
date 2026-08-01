using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class SquarePosWebhookVerifier(IOptions<SquareWebhookOptions> options) : IPosWebhookVerifier
{
    public PosProvider Provider => PosProvider.Square;
    public string SignatureHeaderName => "x-square-hmacsha256-signature";

    public VerifiedPosWebhookEvent Verify(string payload, string signature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(signature);
        var value = options.Value;
        if (string.IsNullOrWhiteSpace(value.SignatureKey) ||
            !Uri.TryCreate(value.NotificationUrl, UriKind.Absolute, out var notificationUri) ||
            notificationUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Square webhook verification is not configured.");

        byte[] supplied;
        try { supplied = Convert.FromBase64String(signature.Trim()); }
        catch (FormatException) { throw new PosWebhookVerificationException(); }
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(value.SignatureKey));
        var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(notificationUri.AbsoluteUri + payload));
        if (supplied.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(supplied, expected))
            throw new PosWebhookVerificationException();

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            return new VerifiedPosWebhookEvent(
                PosProvider.Square,
                Required(root, "event_id", 300),
                Required(root, "type", 200),
                Required(root, "merchant_id", 200),
                payload);
        }
        catch (JsonException) { throw new PosWebhookVerificationException(); }
        catch (ArgumentException) { throw new PosWebhookVerificationException(); }
    }

    private static string Required(JsonElement root, string propertyName, int maximumLength)
    {
        if (!root.TryGetProperty(propertyName, out var property) || string.IsNullOrWhiteSpace(property.GetString()))
            throw new ArgumentException("Required provider metadata is missing.");
        var value = property.GetString()!.Trim();
        return value.Length <= maximumLength ? value : throw new ArgumentException("Provider metadata exceeds the supported length.");
    }
}

public sealed class PosWebhookVerificationException : Exception
{
    public PosWebhookVerificationException() : base("The provider webhook signature or payload is invalid.") { }
}
