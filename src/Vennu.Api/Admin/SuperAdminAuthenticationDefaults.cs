namespace Vennu.Api.Admin;

public static class SuperAdminAuthenticationDefaults
{
    public const string AuthenticationScheme = "SuperAdminApiKey";
    public const string AuthorizationPolicy = "SuperAdmin";
    public const string HeaderName = "X-Vennu-Admin-Key";
}

