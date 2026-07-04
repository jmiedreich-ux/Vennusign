using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Display;

public class ScreenHeartbeatRequest
{
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Online";
}
