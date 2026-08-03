using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Vennu.Api.Configuration;

namespace Vennu.Api.Tests.Screens;

[Collection(DisplayPairingCorsCollection.Name)]
[Trait("Category", "Unit")]
public sealed class DisplayPairingCorsTests : IClassFixture<DisplayPairingCorsFactory>
{
    private readonly DisplayPairingCorsFactory factory;

    public DisplayPairingCorsTests(DisplayPairingCorsFactory factory) => this.factory = factory;

    [Fact]
    public async Task PairingPreflight_AllowsLocalDisplayOrigins()
    {
        using var client = factory.CreateClient();
        foreach (var origin in new[] { "http://localhost:5175", "https://localhost:5175" })
        {
            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/screens");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "content-type");

            using var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(origin, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
            Assert.Contains("POST", response.Headers.GetValues("Access-Control-Allow-Methods"));
        }
    }

    [Fact]
    public void DevelopmentOrigins_IncludeDisplayWithoutWildcard()
    {
        Assert.Contains("http://localhost:5175", DevelopmentCorsOrigins.Values);
        Assert.Contains("https://localhost:5175", DevelopmentCorsOrigins.Values);
        Assert.DoesNotContain("*", DevelopmentCorsOrigins.Values);
    }
}

[CollectionDefinition(DisplayPairingCorsCollection.Name, DisableParallelization = true)]
public sealed class DisplayPairingCorsCollection
{
    public const string Name = "Display pairing CORS factory";
}

public sealed class DisplayPairingCorsFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        for (var index = 0; index < DevelopmentCorsOrigins.Values.Length; index++)
        {
            builder.UseSetting($"Cors:AllowedOrigins:{index}", DevelopmentCorsOrigins.Values[index]);
        }
    }
}
