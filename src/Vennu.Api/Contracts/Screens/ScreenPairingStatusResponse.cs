namespace Vennu.Api.Contracts.Screens;

public class ScreenPairingStatusResponse
{
    public bool Linked { get; set; }

    public Guid? ScreenId { get; set; }
}
