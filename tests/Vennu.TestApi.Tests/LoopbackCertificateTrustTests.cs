using Vennu.TestApi;
using Xunit;

namespace Vennu.TestApi.Tests;

/// <summary>
/// The rule that replaced trusting a certificate on the machine.
///
/// The UI suite failed entirely on every branch for months — not one assertion ran, because this
/// service's HttpClient could not validate the ASP.NET Core development certificate. The obvious
/// fix, `dotnet dev-certs https --trust` in CI, hangs: it raises a Windows certificate-store
/// prompt and a headless runner has nobody to answer it.
///
/// So the decision moved into the process, and these are the two halves of it. Either half alone
/// would be a hole, and the whole point of this file is that widening it has to be deliberate.
/// </summary>
[Trait("Category", "Unit")]
public sealed class LoopbackCertificateTrustTests
{
    [Theory]
    [InlineData("https://localhost:7138/api/test/seed")]
    [InlineData("https://127.0.0.1:7138/api/test/seed")]
    [InlineData("https://[::1]:7138/api/test/seed")]
    [InlineData("https://127.0.0.2:7138/")]
    public void OnLoopback_WhenConfigured_ItAccepts(string uri) =>
        Assert.True(LoopbackCertificateTrust.Allows(configured: true, new Uri(uri)));

    [Theory]
    [InlineData("https://vennusign-dev-api.azurewebsites.net/api/test/seed")]
    [InlineData("https://qa.vennusign.com/")]
    [InlineData("https://localhost.attacker.example/")]
    public void OffLoopback_ItRefusesEvenWhenConfigured(string uri) =>
        // The setting is not permission to accept any certificate from anyone. A base URL pointed
        // at a real host by mistake — or by an environment variable somebody meant for another
        // service — still validates its chain the ordinary way.
        Assert.False(LoopbackCertificateTrust.Allows(configured: true, new Uri(uri)));

    [Fact]
    public void WithoutTheSetting_ItRefusesEvenOnLoopback() =>
        // Off unless something turns it on. Nothing is inherited from an environment name or a
        // build configuration, either of which can drift without anyone deciding it.
        Assert.False(LoopbackCertificateTrust.Allows(configured: false, new Uri("https://localhost:7138/")));

    [Fact]
    public void WithNoRequestUri_ItRefuses() =>
        // A handler can be asked about a request that carries no URI. "Unknown" is not "local".
        Assert.False(LoopbackCertificateTrust.Allows(configured: true, null));

    [Fact]
    public void ARelativeUriIsNotLoopback() =>
        // Uri.IsLoopback throws on a relative URI rather than returning false, so the guard has to
        // come first. Without it this refusal is an unhandled exception inside a TLS callback.
        Assert.False(LoopbackCertificateTrust.Allows(configured: true, new Uri("/api/test/seed", UriKind.Relative)));

    [Fact]
    public void TheOptionIsOffByDefault() =>
        // The default is what every environment gets that never thought about this, including any
        // future one. It has to be the safe answer.
        Assert.False(new TestApiOptions().AllowUntrustedLoopbackCertificate);
}
