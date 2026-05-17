namespace Vennu.Data.Models;

public class ScreenPairingCode
{
    public string Code { get; set; } = string.Empty;

    public Guid? VenueId { get; set; }

    public Guid ScreenId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsClaimed { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? ClaimedAt { get; set; }
}
