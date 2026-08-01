namespace Vennu.Api.Pos;

public sealed class SquareWebhookOptions
{
    public const string SectionName = "Square:Webhooks";
    public string SignatureKey { get; set; } = string.Empty;
    public string NotificationUrl { get; set; } = string.Empty;
}
