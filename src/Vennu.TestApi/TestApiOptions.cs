namespace Vennu.TestApi;

public sealed class TestApiOptions
{
    public const string SectionName = "TestApi";
    public string ProductApiBaseUrl { get; set; } = "https://localhost:7138";
    public string ApiKey { get; set; } = "";
    public string ProductAutomationKey { get; set; } = "";
}
