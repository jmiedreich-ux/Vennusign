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

public sealed class ToastInventoryOptions
{
    public const string SectionName = "Toast:Inventory";
    public string Endpoint { get; set; } = "https://ws-api.toasttab.com/stock/v1/inventory/search";
    public int MaximumItemsPerRequest { get; set; } = 100;
}

public sealed class ToastPollingOptions
{
    public const string SectionName = "Toast:Polling";
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan InterConnectionDelay { get; set; } = TimeSpan.FromMilliseconds(250);
    public TimeSpan InitialFailureBackoff { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan MaximumFailureBackoff { get; set; } = TimeSpan.FromHours(1);
}
