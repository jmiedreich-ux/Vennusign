namespace Vennu.Api.TestAutomation;

public sealed class TestAutomationOptions
{
    public const string SectionName = "TestAutomation";
    public const string HeaderName = "X-Vennusign-Test-Automation-Key";
    public string ApiKey { get; set; } = "";
    public HashSet<string> Scopes { get; set; } = new(StringComparer.Ordinal);
    public HashSet<Guid> AvailabilityVenueIds { get; set; } = [];
    public HashSet<Guid> ResetVenueIds { get; set; } = [];

    /// <summary>
    /// Venues that may be given room for a whole UI run.
    ///
    /// Separate from <see cref="ResetVenueIds"/> on purpose. Reset WIPES a venue, so it is allowed
    /// only on the scale venue, which one test owns at a time. Raising a ceiling destroys nothing,
    /// so it is allowed on the shared venue as well - the one all 98 seeds fill, and the only one
    /// that ever needed it.
    ///
    /// Giving headroom the reset scope instead would have handed every seed the power to wipe a
    /// venue other tests were using.
    /// </summary>
    public HashSet<Guid> HeadroomVenueIds { get; set; } = [];
    public HashSet<Guid> HistoryVenueIds { get; set; } = [];
}
