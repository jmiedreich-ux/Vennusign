namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerAuthenticationOptions
{
    public const string SectionName = "CustomerAuthentication";
    public TimeSpan AbsoluteSessionLifetime { get; set; } = TimeSpan.FromDays(30);
    public TimeSpan IdleSessionLifetime { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan SessionTouchInterval { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan EmailLinkLifetime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RecentAuthenticationWindow { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Whether a session must reach <see cref="Vennu.Core.Models.CustomerAuthenticationAssurance.Strong"/>
    /// (TOTP step-up, or Passkey which is Strong on login) before
    /// <see cref="CustomerAuthenticationDefaults.MfaSatisfiedAuthorizationPolicy"/>-protected endpoints allow
    /// access. False only in dev/stage; <see cref="CustomerAuthenticationOptionsValidator"/> refuses to start
    /// the app in Production with this set to false, per decisions.md #7-#8 in
    /// docs/design/approved/authentication.
    /// </summary>
    public bool RequireMfa { get; set; } = true;

    public Uri FrontendOrigin { get; set; } = new("https://app.vennu.com");
    public CustomerOidcProviderOptions Google { get; set; } = new();
    public CustomerOidcProviderOptions Apple { get; set; } = new();
    public CustomerEntraOptions Entra { get; set; } = new();
    public CustomerEmailDeliveryOptions EmailDelivery { get; set; } = new();
    public CustomerPasskeyOptions Passkeys { get; set; } = new();
}

public sealed class CustomerPasskeyOptions
{
    public string ServerDomain { get; set; } = "app.vennu.com";
    public HashSet<string> Origins { get; set; } = [];
}

public sealed class CustomerOidcProviderOptions
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Microsoft Entra External ID (CIAM) tenant that brokers "Sign in with Vennusign" (its own
/// local account) and, in future, Google/Apple federation - see
/// docs/design/approved/authentication/decisions.md. Not used for Google/Apple yet: those still
/// run through <see cref="CustomerAuthenticationOptions.Google"/> /
/// <see cref="CustomerAuthenticationOptions.Apple"/> directly until that migration happens.
/// </summary>
public sealed class CustomerEntraOptions
{
    public bool Enabled { get; set; }

    /// <summary>The CIAM subdomain, e.g. "vennusign" for vennusign.ciamlogin.com.</summary>
    public string CiamDomain { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    public string Authority => $"https://{CiamDomain}.ciamlogin.com/{TenantId}/v2.0";
}

public sealed class CustomerEmailDeliveryOptions
{
    public bool Enabled { get; set; }
    public Uri? Endpoint { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}
