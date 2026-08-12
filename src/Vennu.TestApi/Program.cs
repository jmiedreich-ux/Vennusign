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
});
builder.Services.AddScoped<SeedService>();

var app = builder.Build();
app.UseMiddleware<TestApiAuthenticationMiddleware>();
app.MapControllers();
app.MapGet("/health/version", () => Results.Ok(new { service = "Vennu.TestApi", status = "ok" }));
app.Run();

public partial class Program;
