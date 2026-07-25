using Microsoft.AspNetCore.SignalR;
using Vennu.Api.Hubs;
using Vennu.Api.Notifications;

namespace Vennu.Api.Tests.Notifications;

public class SignalRScreenUpdateNotifierTests
{
    private static readonly Guid ScreenId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid VenueId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task ScreenContentUpdatedTargetsScreenGroup()
    {
        var (notifier, recorder) = CreateNotifier();
        var payload = new { layout = "default" };

        await notifier.NotifyScreenContentUpdatedAsync(ScreenId, payload);

        AssertMessage(recorder, $"screen:{ScreenId}", "ContentUpdated", payload);
    }

    [Fact]
    public async Task VenueContentUpdatedTargetsVenueGroup()
    {
        var (notifier, recorder) = CreateNotifier();
        var payload = new { layout = "default" };

        await notifier.NotifyVenueContentUpdatedAsync(VenueId, payload);

        AssertMessage(recorder, $"venue:{VenueId}", "ContentUpdated", payload);
    }

    [Fact]
    public async Task ScreenThemeUpdatedTargetsScreenGroup()
    {
        var (notifier, recorder) = CreateNotifier();
        var theme = new { font = "Inter" };

        await notifier.NotifyScreenThemeUpdatedAsync(ScreenId, theme);

        AssertMessage(recorder, $"screen:{ScreenId}", "ThemeUpdated", theme);
    }

    [Fact]
    public async Task VenueThemeUpdatedTargetsVenueGroup()
    {
        var (notifier, recorder) = CreateNotifier();
        var theme = new { font = "Inter" };

        await notifier.NotifyVenueThemeUpdatedAsync(VenueId, theme);

        AssertMessage(recorder, $"venue:{VenueId}", "ThemeUpdated", theme);
    }

    [Fact]
    public async Task ScreenItemAvailabilityTargetsScreenGroup()
    {
        var (notifier, recorder) = CreateNotifier();

        await notifier.NotifyScreenItemAvailabilityChangedAsync(ScreenId, "item-1", false);

        AssertMessage(recorder, $"screen:{ScreenId}", "ItemAvailabilityChanged", "item-1", false);
    }

    [Fact]
    public async Task VenueItemAvailabilityTargetsVenueGroup()
    {
        var (notifier, recorder) = CreateNotifier();

        await notifier.NotifyVenueItemAvailabilityChangedAsync(VenueId, "item-1", true);

        AssertMessage(recorder, $"venue:{VenueId}", "ItemAvailabilityChanged", "item-1", true);
    }

    [Fact]
    public async Task ScreenSyncTickTargetsScreenGroup()
    {
        var (notifier, recorder) = CreateNotifier();

        await notifier.NotifyScreenSyncTickAsync(ScreenId, 123456789L);

        AssertMessage(recorder, $"screen:{ScreenId}", "SyncTick", 123456789L);
    }

    [Fact]
    public async Task VenueSyncTickTargetsVenueGroup()
    {
        var (notifier, recorder) = CreateNotifier();

        await notifier.NotifyVenueSyncTickAsync(VenueId, 987654321L);

        AssertMessage(recorder, $"venue:{VenueId}", "SyncTick", 987654321L);
    }

    private static (IScreenUpdateNotifier Notifier, RecordingHubClients Recorder) CreateNotifier()
    {
        var recorder = new RecordingHubClients();
        var context = new RecordingHubContext(recorder);
        return (new SignalRScreenUpdateNotifier(context), recorder);
    }

    private static void AssertMessage(RecordingHubClients recorder, string group, string method, params object?[] args)
    {
        var message = Assert.Single(recorder.Messages);
        Assert.Equal(group, message.Group);
        Assert.Equal(method, message.Method);
        Assert.Equal(args, message.Args);
    }

    private sealed record RecordedMessage(string Group, string Method, object?[] Args);

    private sealed class RecordingClientProxy(string group, List<RecordedMessage> messages) : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            messages.Add(new RecordedMessage(group, method, args));
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingHubClients : IHubClients
    {
        public List<RecordedMessage> Messages { get; } = [];

        public IClientProxy All => new RecordingClientProxy("all", Messages);
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new RecordingClientProxy("all", Messages);
        public IClientProxy Client(string connectionId) => new RecordingClientProxy($"client:{connectionId}", Messages);
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new RecordingClientProxy("clients", Messages);
        public IClientProxy Group(string groupName) => new RecordingClientProxy(groupName, Messages);
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new RecordingClientProxy(groupName, Messages);
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => new RecordingClientProxy(string.Join(',', groupNames), Messages);
        public IClientProxy User(string userId) => new RecordingClientProxy($"user:{userId}", Messages);
        public IClientProxy Users(IReadOnlyList<string> userIds) => new RecordingClientProxy("users", Messages);
    }

    private sealed class RecordingHubContext(RecordingHubClients clients) : IHubContext<VennuHub>
    {
        public IHubClients Clients { get; } = clients;
        public IGroupManager Groups { get; } = new NoOpGroupManager();
    }

    private sealed class NoOpGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
