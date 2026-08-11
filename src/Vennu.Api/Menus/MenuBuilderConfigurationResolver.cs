using Microsoft.Extensions.Options;
using Vennu.Data.Repositories;

namespace Vennu.Api.Menus;

public sealed record ResolvedMenuBuilderConfiguration(
    long ImportFileSizeLimitBytes,
    double PublishRetrySilenceThresholdSeconds,
    int HistoryRetentionDepth);

public sealed class MenuBuilderConfigurationResolver(
    IContentRepository content,
    IOptionsMonitor<MenuBuilderOptions> options)
{
    public async Task<ResolvedMenuBuilderConfiguration> ResolveAsync(Guid venueId, CancellationToken cancellationToken)
    {
        var configured = await content.GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var defaults = options.CurrentValue;
        return new ResolvedMenuBuilderConfiguration(
            configured.TryGetValue(MenuCeilings.ImportFileBytes, out var importBytes)
                ? importBytes
                : defaults.ImportFileSizeLimitBytes,
            configured.TryGetValue(MenuCeilings.PublishRetrySilenceSeconds, out var retrySeconds)
                ? retrySeconds
                : defaults.PublishRetrySilenceThreshold.TotalSeconds,
            configured.TryGetValue(MenuCeilings.HistoryRetention, out var retention)
                ? retention
                : defaults.HistoryRetentionDepth);
    }
}
