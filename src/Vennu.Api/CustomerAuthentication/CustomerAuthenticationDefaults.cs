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

    /// <summary>
    /// Entra External ID tenant. Currently only used for "Sign in with Vennusign" (Entra's own
    /// local account) - Google/Apple still route through <see cref="GoogleScheme"/> /
    /// <see cref="AppleScheme"/> directly. See
    /// docs/design/approved/authentication/decisions.md for the target state where all three
    /// route through this scheme.
    /// </summary>
    public const string EntraScheme = "CustomerEntra";
    public const string SessionCookieName = "__Host-Vennusign.CustomerSession";
    public const string LegacySessionCookieName = "__Host-Vennu.CustomerSession";
}
