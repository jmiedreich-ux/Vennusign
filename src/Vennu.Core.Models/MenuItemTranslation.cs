namespace Vennu.Core.Models;

public sealed class MenuItemTranslation
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public Guid MenuItemId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsAutoTranslated { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
