namespace Vennu.Api.Contracts.Screens;

public class CreateScreenPairingCodeResponse
{
    public string Code { get; set; } = string.Empty;

    public Guid ScreenId { get; set; }

    public DateTime ExpiresAt { get; set; }
}
