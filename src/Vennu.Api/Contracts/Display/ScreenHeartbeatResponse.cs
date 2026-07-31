namespace Vennu.Api.Contracts.Display;

public class ScreenHeartbeatResponse
{
    public Guid ScreenId { get; set; }

    public string Status { get; set; } = "Online";

    public DateTime LastSeenUtc { get; set; }

    public string? Platform { get; set; }

    public string? AppVersion { get; set; }
}
