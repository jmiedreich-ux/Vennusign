namespace Vennu.Api.TestAgent;

public sealed class TestAgentOptions
{
    public const string SectionName = "TestAgent";
    public bool Enabled { get; set; }
    public string Model { get; set; } = "gpt-5-mini";
    public string? OpenAiApiKey { get; set; }
    public string BackOfficeBaseUrl { get; set; } = "http://localhost:5173";
    public int MaximumActions { get; set; } = 30;
    public int MaximumMinutes { get; set; } = 20;
}
