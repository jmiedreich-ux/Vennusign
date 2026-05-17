using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Screens;

public class CreateScreenPairingCodeRequest
{
    [Required]
    public Guid ScreenId { get; set; }
}
