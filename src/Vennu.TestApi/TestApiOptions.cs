namespace Vennu.TestApi;

public sealed class TestApiOptions
{
    public const string SectionName = "TestApi";
    public string ProductApiBaseUrl { get; set; } = "https://localhost:7138";
    public string ApiKey { get; set; } = "";
    public string ProductAutomationKey { get; set; } = "";

    /// <summary>
    /// Accept the ASP.NET Core development certificate when calling the product API on loopback.
    ///
    /// Default false, and it must be turned on deliberately. See
    /// <see cref="LoopbackCertificateTrust"/> for why this is decided here rather than by trusting
    /// the certificate on the machine - and why "loopback" is half the rule, not a comment.
    /// </summary>
    public bool AllowUntrustedLoopbackCertificate { get; set; }
}
