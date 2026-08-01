namespace Vennu.Api.Pos;

public sealed class CloverWebhookOptions
{
    public const string SectionName = "Clover:Webhooks";

    public string AppId { get; set; } = string.Empty;
    public string AuthCode { get; set; } = string.Empty;
}
