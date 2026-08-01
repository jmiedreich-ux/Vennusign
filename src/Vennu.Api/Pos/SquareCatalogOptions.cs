namespace Vennu.Api.Pos;

public sealed class SquareCatalogOptions
{
    public const string SectionName = "Square:Catalog";
    public string Endpoint { get; set; } = "https://connect.squareup.com/v2/catalog/list";
    public string ApiVersion { get; set; } = "2026-07-15";
}
