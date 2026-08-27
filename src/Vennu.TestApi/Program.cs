using Vennu.TestApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOptions<TestApiOptions>()
    .Bind(builder.Configuration.GetSection(TestApiOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.ProductApiBaseUrl, UriKind.Absolute, out _), "A product API base URL is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "A Test API key is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ProductAutomationKey), "A product automation key is required.")
    .ValidateOnStart();
builder.Services.AddHttpClient<ProductApiClient>((services, client) =>
{
    var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<TestApiOptions>>().Value;
    client.BaseAddress = new Uri(options.ProductApiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(services =>
{
    var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<TestApiOptions>>().Value;
    // Not "accept anything": the callback refuses everything that is not loopback even when the
    // setting is on, so a base URL pointed at a real host by mistake still validates its chain the
    // ordinary way. The handler lives in LoopbackCertificateTrust so its own tests use this one.
    return LoopbackCertificateTrust.CreateHandler(options.AllowUntrustedLoopbackCertificate);
});
builder.Services.AddScoped<SeedService>();

var app = builder.Build();
app.UseMiddleware<TestApiAuthenticationMiddleware>();
app.MapControllers();
app.MapGet("/health/version", () => Results.Ok(new { service = "Vennu.TestApi", status = "ok" }));
app.Run();

public partial class Program;
