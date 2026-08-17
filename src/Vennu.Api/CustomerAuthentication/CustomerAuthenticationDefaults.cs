namespace Vennu.Api.CustomerAuthentication;

public static class CustomerAuthenticationDefaults
{
    public const string AuthenticationScheme = "CustomerSession";
    public const string AuthorizationPolicy = "CustomerAuthenticated";

    /// <summary>
    /// Session-authenticated AND, unless <see cref="CustomerAuthenticationOptions.RequireMfa"/> is false,
    /// carries Strong assurance (Passkey login, or TOTP/recovery-code step-up). Use this on real account/
    /// business actions. Do not use it on the step-up endpoints themselves, or on session-introspection
    /// endpoints a client needs to call before completing step-up — those stay on
    /// <see cref="AuthorizationPolicy"/>.
    /// </summary>
    public const string MfaSatisfiedAuthorizationPolicy = "CustomerMfaSatisfied";
    public const string ExternalCookieScheme = "CustomerExternal";
    public const string GoogleScheme = "CustomerGoogle";
    public const string AppleScheme = "CustomerApple";
    public const string SessionCookieName = "__Host-Vennusign.CustomerSession";
    public const string LegacySessionCookieName = "__Host-Vennu.CustomerSession";
}
