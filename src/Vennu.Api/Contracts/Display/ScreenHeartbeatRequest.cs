using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Display;

public class ScreenHeartbeatRequest
{
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Online";

    [StringLength(50)]
    public string? Platform { get; set; }

    [StringLength(50)]
    public string? AppVersion { get; set; }
}
