using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class ToastPosWebhookVerifier(IOptions<ToastWebhookOptions> options) : IPosWebhookVerifier
{
    public PosProvider Provider => PosProvider.Toast;
    public string SignatureHeaderName => "Toast-Signature";

    public VerifiedPosWebhookEvent Verify(string payload, string signature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(signature);
        try
        {
            using var document = JsonDocument.Parse(payload, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            var timestamp = Required(root, "timestamp", 100);
            var category = Required(root, "eventCategory", 100);
            var eventType = Required(root, "eventType", 100);
            var eventId = RequiredGuid(root, "guid");
            if (!root.TryGetProperty("details", out var details) || details.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("Toast webhook details are missing.");
            var restaurantId = RequiredGuid(details, "restaurantGuid");
            var secret = category switch
            {
                "menus" => options.Value.MenusSecret,
                "stock" => options.Value.StockSecret,
                _ => throw new PosWebhookVerificationException()
            };
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Toast webhook verification is not configured.");

            byte[] supplied;
            try { supplied = Convert.FromBase64String(signature.Trim()); }
            catch (FormatException) { throw new PosWebhookVerificationException(); }
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload + timestamp));
            if (supplied.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(supplied, expected))
                throw new PosWebhookVerificationException();

            return new VerifiedPosWebhookEvent(PosProvider.Toast, eventId, eventType, restaurantId, payload);
        }
        catch (JsonException) { throw new PosWebhookVerificationException(); }
        catch (ArgumentException) { throw new PosWebhookVerificationException(); }
    }

    private static string Required(JsonElement root, string propertyName, int maximumLength)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.GetString()))
            throw new ArgumentException("Required Toast webhook metadata is missing.");
        var value = property.GetString()!.Trim();
        return value.Length <= maximumLength ? value : throw new ArgumentException("Toast webhook metadata is too long.");
    }

    private static string RequiredGuid(JsonElement root, string propertyName)
    {
        var value = Required(root, propertyName, 36);
        return Guid.TryParse(value, out var parsed) ? parsed.ToString() : throw new ArgumentException("Toast webhook GUID is invalid.");
    }
}
