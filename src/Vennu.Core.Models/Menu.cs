namespace Vennu.Core.Models;

public sealed class Menu
{
    public Guid Id { get; set; }

    public Guid VenueId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string? DailySpecial { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
