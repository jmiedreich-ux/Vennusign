using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Screens;

public class ClaimScreenPairingCodeRequest
{
    [Required]
    public Guid VenueId { get; set; }
}
