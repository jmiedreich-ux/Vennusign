namespace Vennu.Api.Pos;

public sealed class CloverCatalogOptions
{
    public const string SectionName = "Clover:Catalog";

    public string BaseUrl { get; set; } = "https://api.clover.com";
    public string CurrencyCode { get; set; } = "USD";
    public int PageSize { get; set; } = 1000;
}
