using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Screens;

public class RegisterScreenRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(50)]
    public string? Platform { get; set; }

    [StringLength(50)]
    public string? AppVersion { get; set; }

    [Range(1, 16384)] public int WidthPixels { get; set; } = 1920;
    [Range(1, 16384)] public int HeightPixels { get; set; } = 1080;
}
