using Microsoft.Extensions.Options;
using Vennu.Api.Menus;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Data.Repositories;
using Xunit;

namespace Vennu.Api.Tests.Configuration;

public sealed class MenuBuilderConfigurationResolverTests
{
    [Fact]
    public async Task Venue_allowances_override_runtime_defaults_independently()
    {
        var repository = new FakeContentRepository();
        repository.Ceilings[MenuCeilings.ImportFileBytes] = 9_000_000;
        repository.Ceilings[MenuCeilings.PublishRetrySilenceSeconds] = 45;
        repository.Ceilings[MenuCeilings.HistoryRetention] = 80;
        var resolver = new MenuBuilderConfigurationResolver(repository, Monitor(new MenuBuilderOptions
        {
            ImportFileSizeLimitBytes = 5_000_000,
            PublishRetrySilenceThreshold = TimeSpan.FromSeconds(30),
            HistoryRetentionDepth = 50
        }));

        var resolved = await resolver.ResolveAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(9_000_000, resolved.ImportFileSizeLimitBytes);
        Assert.Equal(45, resolved.PublishRetrySilenceThresholdSeconds);
        Assert.Equal(80, resolved.HistoryRetentionDepth);
    }

    [Fact]
    public async Task Missing_allowances_use_runtime_configuration_not_hardcoded_view_values()
    {
        var resolver = new MenuBuilderConfigurationResolver(new FakeContentRepository(), Monitor(new MenuBuilderOptions
        {
            ImportFileSizeLimitBytes = 7_000_000,
            PublishRetrySilenceThreshold = TimeSpan.FromSeconds(37),
            HistoryRetentionDepth = 61
        }));

        var resolved = await resolver.ResolveAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(7_000_000, resolved.ImportFileSizeLimitBytes);
        Assert.Equal(37, resolved.PublishRetrySilenceThresholdSeconds);
        Assert.Equal(61, resolved.HistoryRetentionDepth);
    }

    private static IOptionsMonitor<T> Monitor<T>(T value) => new FixedOptionsMonitor<T>(value);

    private sealed class FixedOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
