namespace Vennu.Api.Menus;

/// <summary>
/// Runtime-owned Menu Builder limits. The later tier and venue resolvers may
/// narrow these values; views consume the resolved response and never copy the
/// defaults.
/// </summary>
public sealed class MenuBuilderOptions
{
    public const string SectionName = "Menus:Builder";

    public long ImportFileSizeLimitBytes { get; set; } = 5 * 1024 * 1024;

    public TimeSpan PublishRetrySilenceThreshold { get; set; } = TimeSpan.FromSeconds(30);

    public int HistoryRetentionDepth { get; set; } = 50;
}
