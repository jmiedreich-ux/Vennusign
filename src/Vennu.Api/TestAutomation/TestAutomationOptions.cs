namespace Vennu.Api.TestAutomation;

public sealed class TestAutomationOptions
{
    public const string SectionName = "TestAutomation";
    public const string HeaderName = "X-Vennusign-Test-Automation-Key";
    public string ApiKey { get; set; } = "";
    public HashSet<string> Scopes { get; set; } = new(StringComparer.Ordinal);
    public HashSet<Guid> AvailabilityVenueIds { get; set; } = [];
    public HashSet<Guid> ResetVenueIds { get; set; } = [];
    public HashSet<Guid> HistoryVenueIds { get; set; } = [];
}
