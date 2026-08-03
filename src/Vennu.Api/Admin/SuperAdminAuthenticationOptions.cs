using Microsoft.AspNetCore.Authentication;

namespace Vennu.Api.Admin;

public sealed class SuperAdminAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "SuperAdmin";

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

