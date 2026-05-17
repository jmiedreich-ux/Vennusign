namespace Vennu.Api.Contracts.Screens;

public class ClaimScreenPairingCodeResponse
{
    public bool Linked { get; set; }

    public Guid ScreenId { get; set; }

    public Guid VenueId { get; set; }
}
