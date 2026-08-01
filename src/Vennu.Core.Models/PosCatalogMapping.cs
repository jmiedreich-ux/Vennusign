namespace Vennu.Core.Models;

public enum PosCatalogEntityType
{
    Menu = 1,
    Category = 2,
    Item = 3,
    Modifier = 4
}

public sealed class PosCatalogMapping
{
    public Guid Id { get; set; }
    public Guid VenueId { get; set; }
    public PosProvider Provider { get; set; }
    public PosCatalogEntityType EntityType { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid LocalEntityId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
