namespace Vennu.Api.Contracts.Display;

public class DisplayContentResponse
{
    public Guid ScreenId { get; set; }

    public Guid? VenueId { get; set; }

    public string ScreenKey { get; set; } = string.Empty;

    public string ScreenName { get; set; } = string.Empty;

    public string Status { get; set; } = "Offline";

    public DateTime? LastSeenUtc { get; set; }

    public string Layout { get; set; } = "default";
}
