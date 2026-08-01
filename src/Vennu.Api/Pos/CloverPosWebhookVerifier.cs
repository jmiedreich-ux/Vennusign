using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class CloverPosWebhookVerifier(IOptions<CloverWebhookOptions> options) : IPosWebhookVerifier
{
    private const int MaximumMerchants = 100;
    private const int MaximumEvents = 1000;

    public PosProvider Provider => PosProvider.Clover;
    public string SignatureHeaderName => "X-Clover-Auth";

    public VerifiedPosWebhookEvent Verify(string payload, string signature)
    {
        var events = VerifyMany(payload, signature);
        return events.Count == 1 ? events.Single() : throw new PosWebhookVerificationException();
    }

    public IReadOnlyCollection<VerifiedPosWebhookEvent> VerifyMany(string payload, string signature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(signature);
        var value = options.Value;
        if (string.IsNullOrWhiteSpace(value.AppId) || string.IsNullOrWhiteSpace(value.AuthCode))
            throw new InvalidOperationException("Clover webhook verification is not configured.");
        if (!FixedTimeEquals(value.AuthCode.Trim(), signature.Trim())) throw new PosWebhookVerificationException();

        try
        {
            using var document = JsonDocument.Parse(payload, new JsonDocumentOptions { MaxDepth = 32 });
            var root = document.RootElement;
            if (!string.Equals(Required(root, "appId", 200), value.AppId.Trim(), StringComparison.Ordinal) ||
                !root.TryGetProperty("merchants", out var merchants) || merchants.ValueKind != JsonValueKind.Object)
                throw new PosWebhookVerificationException();

            var merchantProperties = merchants.EnumerateObject().ToArray();
            if (merchantProperties.Length is 0 or > MaximumMerchants) throw new PosWebhookVerificationException();
            var results = new List<VerifiedPosWebhookEvent>();
            var eventCount = 0;
            foreach (var merchant in merchantProperties)
            {
                var merchantId = Bounded(merchant.Name, 200);
                if (merchant.Value.ValueKind != JsonValueKind.Array) throw new PosWebhookVerificationException();
                foreach (var update in merchant.Value.EnumerateArray())
                {
                    if (++eventCount > MaximumEvents) throw new PosWebhookVerificationException();
                    var objectId = Required(update, "objectId", 300);
                    if (!objectId.StartsWith("I:", StringComparison.Ordinal) || objectId.Length == 2) continue;
                    var operation = Required(update, "type", 20).ToUpperInvariant();
                    if (operation is not ("CREATE" or "UPDATE" or "DELETE")) throw new PosWebhookVerificationException();
                    if (!update.TryGetProperty("ts", out var timestamp) || !timestamp.TryGetInt64(out var timestampMs) || timestampMs <= 0)
                        throw new PosWebhookVerificationException();
                    var normalized = JsonSerializer.Serialize(new
                    {
                        objectId,
                        type = operation,
                        ts = timestampMs
                    });
                    var identity = $"{value.AppId.Trim()}|{merchantId}|{objectId}|{operation}|{timestampMs}";
                    results.Add(new VerifiedPosWebhookEvent(
                        PosProvider.Clover,
                        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity))).ToLowerInvariant(),
                        $"inventory.item.{operation.ToLowerInvariant()}",
                        merchantId,
                        normalized));
                }
            }
            return results;
        }
        catch (JsonException) { throw new PosWebhookVerificationException(); }
        catch (ArgumentException) { throw new PosWebhookVerificationException(); }
    }

    private static bool FixedTimeEquals(string expected, string supplied)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        return expectedBytes.Length == suppliedBytes.Length && CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    private static string Required(JsonElement root, string propertyName, int maximumLength)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            throw new ArgumentException("Required Clover webhook metadata is missing.");
        return Bounded(property.GetString(), maximumLength);
    }

    private static string Bounded(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Required Clover webhook metadata is missing.");
        var normalized = value.Trim();
        return normalized.Length <= maximumLength ? normalized : throw new ArgumentException("Clover webhook metadata is too long.");
    }
}
