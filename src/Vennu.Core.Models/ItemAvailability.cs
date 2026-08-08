namespace Vennu.Core.Models;

/// <summary>
/// Availability ("86") for one item at one venue. It is a fact about the world
/// that is already true: it commits instantly, never joins a draft queue,
/// survives a publish, and stays off until a person turns it back on.
/// </summary>
public sealed class ItemAvailability
{
    public Guid VenueId { get; set; }

    public Guid ItemId { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime ChangedUtc { get; set; }

    public string? ChangedBy { get; set; }
}
