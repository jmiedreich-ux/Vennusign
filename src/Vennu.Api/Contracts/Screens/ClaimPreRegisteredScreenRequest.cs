using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Screens;

public sealed class ClaimPreRegisteredScreenRequest
{
    [Required]
    [StringLength(128, MinimumLength = 32)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Platform { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string AppVersion { get; set; } = string.Empty;
}
