using System.Net;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vennu.Api.CustomerAuthentication;
using Vennu.Api.Tests.E2E;

namespace Vennu.Api.Tests.CustomerAuthentication;

[Trait("Category", "Unit")]
public sealed class CustomerAuthenticationSecurityTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public CustomerAuthenticationSecurityTests(VennuApiFactory factory) => this.factory = factory;

    [Theory]
    [InlineData(CustomerAuthenticationDefaults.GoogleScheme, "https://accounts.google.com", "/signin-customer-google")]
    [InlineData(CustomerAuthenticationDefaults.AppleScheme, "https://appleid.apple.com", "/signin-customer-apple")]
    public void OidcSchemes_RequireCodePkceNonceIssuerAudienceAndLifetime(
        string scheme,
        string authority,
        string callbackPath)
    {
        using var scope = factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(scheme);

        Assert.Equal(authority, options.Authority);
        Assert.Equal(callbackPath, options.CallbackPath);
        Assert.Equal("code", options.ResponseType);
        Assert.True(options.UsePkce);
        Assert.True(options.RequireHttpsMetadata);
        Assert.False(options.SaveTokens);
        Assert.True(options.TokenValidationParameters.ValidateIssuer);
        Assert.True(options.TokenValidationParameters.ValidateAudience);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
        Assert.Equal(TimeSpan.FromMinutes(10), options.RemoteAuthenticationTimeout);
        Assert.Contains("openid", options.Scope);
        Assert.Contains("email", options.Scope);
    }

    [Fact]
    public async Task CustomerSession_RequiresPersistedSessionCookie()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/customer-auth/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExternalSignIn_RejectsExternalReturnPathBeforeChallenge()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/customer-auth/external/google?returnPath=https://attacker.example");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void TrustedFrontendReturn_CombinesOriginWithBoundedLocalPath()
    {
        var valid = CustomerReturnUri.TryCreate(
            new Uri("https://localhost:5174"),
            "/onboarding?step=organization",
            out var result);

        Assert.True(valid);
        Assert.Equal("https://localhost:5174/onboarding?step=organization", result.AbsoluteUri);
    }

    [Theory]
    [InlineData("https://attacker.example")]
    [InlineData("//attacker.example")]
    [InlineData("/\\attacker.example")]
    public void TrustedFrontendReturn_RejectsNonLocalPaths(string returnPath)
    {
        Assert.False(CustomerReturnUri.TryCreate(new Uri("https://localhost:5174"), returnPath, out _));
    }

    [Theory]
    [InlineData("http://localhost:5174")]
    [InlineData("https://localhost:5174/base/")]
    [InlineData("https://user@localhost:5174")]
    public void TrustedFrontendReturn_RejectsInvalidOrigins(string origin)
    {
        Assert.False(CustomerReturnUri.IsValidOrigin(new Uri(origin)));
    }

    [Fact]
    public async Task DisabledProvider_ReturnsServiceUnavailableWithoutRedirect()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/customer-auth/external/apple?returnPath=/welcome");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public void EnabledProviderWithoutCredentials_FailsConfigurationValidation()
    {
        var options = new CustomerAuthenticationOptions { Google = new CustomerOidcProviderOptions { Enabled = true } };

        var result = new CustomerAuthenticationOptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures!, failure => failure.Contains("Google:ClientId", StringComparison.Ordinal));
        Assert.Contains(result.Failures!, failure => failure.Contains("Google:ClientSecret", StringComparison.Ordinal));
    }

    [Fact]
    public void InvalidFrontendOrigin_FailsConfigurationValidation()
    {
        var options = new CustomerAuthenticationOptions { FrontendOrigin = new Uri("http://localhost:5174") };

        var result = new CustomerAuthenticationOptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures!, failure => failure.Contains("FrontendOrigin", StringComparison.Ordinal));
    }

    [Fact]
    public void ProductionPasskeys_RejectLocalhostSettings()
    {
        var options = new CustomerAuthenticationOptions
        {
            FrontendOrigin = new Uri("https://app.vennu.com"),
            Passkeys = new CustomerPasskeyOptions { ServerDomain = "localhost", Origins = ["https://localhost:5174"] }
        };
        var result = new CustomerAuthenticationOptionsValidator(false).Validate(null, options);
        Assert.True(result.Failed);
        Assert.Contains(result.Failures!, failure => failure.Contains("Development", StringComparison.Ordinal));
    }

    [Fact]
    public void DevelopmentPasskeys_AcceptDocumentedLocalhostOrigin()
    {
        var options = new CustomerAuthenticationOptions
        {
            FrontendOrigin = new Uri("https://localhost:5174"),
            Passkeys = new CustomerPasskeyOptions { ServerDomain = "localhost", Origins = ["https://localhost:5174"] }
        };
        Assert.False(new CustomerAuthenticationOptionsValidator(true).Validate(null, options).Failed);
    }

    [Fact]
    public void SessionCookie_IsSecureHttpOnlyHostScopedAndSameSiteLax()
    {
        var context = new DefaultHttpContext();

        CustomerSessionCookie.Append(context.Response, "opaque-token", DateTime.UtcNow.AddHours(1));

        var cookie = Assert.Single(context.Response.Headers.SetCookie).ToString();
        Assert.Contains("__Host-Vennusign.CustomerSession=opaque-token", cookie, StringComparison.Ordinal);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/", cookie, StringComparison.OrdinalIgnoreCase);
    }
}
