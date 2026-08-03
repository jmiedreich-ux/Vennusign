using Microsoft.AspNetCore.Authentication;

namespace Vennu.Api.PlatformOperations;

public sealed class PlatformOperationsAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "PlatformOperations";
    public const string LegacySectionName = "SuperAdmin";

    public string ApiKey { get; set; } = string.Empty;

    public List<string> ConfigurationPermissions { get; set; } =
    [
        "read",
        "edit",
        "secrets",
        "import",
        "admin"
    ];
}
