using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vennu.TestApi;
using Xunit;

namespace Vennu.TestApi.Tests;

public sealed class TestApiBoundaryTests
{
    [Fact]
    public async Task Missing_or_wrong_key_is_indistinguishable_and_refused()
    {
        var reached = false;
        var middleware = new TestApiAuthenticationMiddleware(_ => { reached = true; return Task.CompletedTask; });
        var options = Options.Create(new TestApiOptions { ApiKey = "expected" });

        foreach (var supplied in new[] { null, "wrong" })
        {
            reached = false;
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/test/seed";
            if (supplied is not null) context.Request.Headers["X-Vennusign-Test-Api-Key"] = supplied;
            await middleware.InvokeAsync(context, options);
            Assert.False(reached);
            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        }
    }

    [Fact]
    public async Task Correct_key_reaches_the_endpoint()
    {
        var reached = false;
        var middleware = new TestApiAuthenticationMiddleware(_ => { reached = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test/seed";
        context.Request.Headers["X-Vennusign-Test-Api-Key"] = "expected";
        await middleware.InvokeAsync(context, Options.Create(new TestApiOptions { ApiKey = "expected" }));
        Assert.True(reached);
    }

    [Fact]
    public async Task Product_client_sends_product_and_automation_credentials_only_to_the_product_api()
    {
        var handler = new RecordingHandler();
        var client = new ProductApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://product.invalid") },
            Options.Create(new TestApiOptions { ProductAutomationKey = "automation-secret" }));

        await client.SendAsync(HttpMethod.Post, "/ordinary", "venue-token", new { value = 1 }, CancellationToken.None);
        await client.SendAutomationAsync("/automation", new { value = 2 }, CancellationToken.None);

        Assert.Equal("venue-token", handler.Requests[0].Headers.GetValues("X-Vennusign-Back-Office-Token").Single());
        Assert.False(handler.Requests[0].Headers.Contains("X-Vennusign-Test-Automation-Key"));
        Assert.Equal("automation-secret", handler.Requests[1].Headers.GetValues("X-Vennusign-Test-Automation-Key").Single());
        Assert.False(handler.Requests[1].Headers.Contains("X-Vennusign-Back-Office-Token"));
    }

    [Fact]
    public void Deployable_has_no_project_or_package_dependency_on_product_data_layers()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Vennusign.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);
        var project = File.ReadAllText(Path.Combine(directory!.FullName, "src", "Vennu.TestApi", "Vennu.TestApi.csproj"));
        Assert.DoesNotContain("ProjectReference", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Vennu.Data", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Vennu.Api", project, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var copy = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers) copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
            if (request.Content is not null) copy.Content = new StringContent(await request.Content.ReadAsStringAsync(cancellationToken), Encoding.UTF8, "application/json");
            Requests.Add(copy);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
