using System.ComponentModel.DataAnnotations.Schema;

namespace Vennu.Data.Models;

[Table("Venues")]
public class Venue
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Timezone { get; set; } = "UTC";

    public string Type { get; set; } = string.Empty;

    public string PrimaryLanguage { get; set; } = "en";

    public string? SecondaryLanguage { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
