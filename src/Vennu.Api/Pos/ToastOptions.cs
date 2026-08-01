namespace Vennu.Api.Pos;

public sealed class ToastCatalogOptions
{
    public const string SectionName = "Toast:Catalog";
    public string Endpoint { get; set; } = "https://ws-api.toasttab.com/menus/v2/menus";
    public string CurrencyCode { get; set; } = "USD";
}

public sealed class ToastWebhookOptions
{
    public const string SectionName = "Toast:Webhooks";
    public string MenusSecret { get; set; } = string.Empty;
    public string StockSecret { get; set; } = string.Empty;
}
