using System.ComponentModel.DataAnnotations;

namespace Vennu.Api.Contracts.Venues;

public class CreateVenueRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Timezone { get; set; } = "UTC";

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string PrimaryLanguage { get; set; } = "en";

    [StringLength(10)]
    public string? SecondaryLanguage { get; set; }
}
