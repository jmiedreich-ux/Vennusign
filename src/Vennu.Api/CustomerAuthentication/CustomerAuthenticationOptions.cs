namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerAuthenticationOptions
{
    public const string SectionName = "CustomerAuthentication";
    public TimeSpan AbsoluteSessionLifetime { get; set; } = TimeSpan.FromDays(30);
    public TimeSpan IdleSessionLifetime { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan SessionTouchInterval { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan EmailLinkLifetime { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RecentAuthenticationWindow { get; set; } = TimeSpan.FromMinutes(10);
    public Uri FrontendOrigin { get; set; } = new("https://app.vennu.com");
    public CustomerOidcProviderOptions Google { get; set; } = new();
    public CustomerOidcProviderOptions Apple { get; set; } = new();
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

public sealed class CustomerEmailDeliveryOptions
{
    public bool Enabled { get; set; }
    public Uri? Endpoint { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}
