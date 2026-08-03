using Microsoft.Extensions.Configuration;
using Vennu.Api.Billing;
using Vennu.Api.CustomerAuthentication;
using Vennu.Api.Pos;
using Vennu.Api.Webhooks;

namespace Vennu.Api.Tests.Configuration;

public sealed class ProviderConfigurationBindingTests
{
    [Fact]
    public void RegisteredKeyShapesBindExistingProviderOptions()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["CustomerAuthentication:Google:Enabled"] = "true",
            ["CustomerAuthentication:Google:ClientId"] = "google-id",
            ["CustomerAuthentication:Google:ClientSecret"] = "google-secret",
            ["CustomerAuthentication:FrontendOrigin"] = "https://localhost:5174",
            ["Stripe:Revenue:ApiKey"] = "stripe-key",
            ["Stripe:Webhook:SigningSecret"] = "whsec_test",
            ["Stripe:HaasCheckout:PriceIds:starter_kit"] = "price_starter",
            ["Square:OAuth:ApplicationId"] = "square-id",
            ["Square:OAuth:ApplicationSecret"] = "square-secret",
            ["Square:OAuth:Scopes:0"] = "ITEMS_READ",
            ["Toast:Webhooks:MenusSecret"] = "toast-secret",
            ["Clover:OAuth:ClientId"] = "clover-id",
            ["Clover:OAuth:ClientSecret"] = "clover-secret"
        }).Build();

        var authentication = configuration.GetSection(CustomerAuthenticationOptions.SectionName).Get<CustomerAuthenticationOptions>()!;
        var stripeRevenue = configuration.GetSection(StripeRevenueOptions.SectionName).Get<StripeRevenueOptions>()!;
        var stripeWebhook = configuration.GetSection(StripeWebhookOptions.SectionName).Get<StripeWebhookOptions>()!;
        var stripeHaas = configuration.GetSection(StripeHaasCheckoutOptions.SectionName).Get<StripeHaasCheckoutOptions>()!;
        var square = configuration.GetSection(SquareOAuthOptions.SectionName).Get<SquareOAuthOptions>()!;
        var toast = configuration.GetSection(ToastWebhookOptions.SectionName).Get<ToastWebhookOptions>()!;
        var clover = configuration.GetSection(CloverOAuthOptions.SectionName).Get<CloverOAuthOptions>()!;

        Assert.True(authentication.Google.Enabled);
        Assert.Equal("google-id", authentication.Google.ClientId);
        Assert.Equal("google-secret", authentication.Google.ClientSecret);
        Assert.Equal(new Uri("https://localhost:5174"), authentication.FrontendOrigin);
        Assert.Equal("stripe-key", stripeRevenue.ApiKey);
        Assert.Equal("whsec_test", stripeWebhook.SigningSecret);
        Assert.Equal("price_starter", stripeHaas.PriceIds["starter_kit"]);
        Assert.Equal("square-id", square.ApplicationId);
        Assert.Equal("square-secret", square.ApplicationSecret);
        Assert.Equal(["ITEMS_READ"], square.Scopes);
        Assert.Equal("toast-secret", toast.MenusSecret);
        Assert.Equal("clover-id", clover.ClientId);
        Assert.Equal("clover-secret", clover.ClientSecret);
    }
}
