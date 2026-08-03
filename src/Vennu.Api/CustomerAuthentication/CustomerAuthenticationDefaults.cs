namespace Vennu.Api.CustomerAuthentication;

public static class CustomerAuthenticationDefaults
{
    public const string AuthenticationScheme = "CustomerSession";
    public const string AuthorizationPolicy = "CustomerAuthenticated";
    public const string ExternalCookieScheme = "CustomerExternal";
    public const string GoogleScheme = "CustomerGoogle";
    public const string AppleScheme = "CustomerApple";
    public const string SessionCookieName = "__Host-Vennusign.CustomerSession";
    public const string LegacySessionCookieName = "__Host-Vennu.CustomerSession";
}
