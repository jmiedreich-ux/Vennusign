using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace Vennu.Api.TestAgent;

public interface ITestAgentRunService
{
    TestAgentRun Start(StartTestAgentRunRequest request);
    TestAgentRun? Get(Guid id);
    IReadOnlyCollection<TestAgentRun> List();
    bool Cancel(Guid id);
}

public sealed class TestAgentRunService(
    IOptions<TestAgentOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<TestAgentRunService> logger) : ITestAgentRunService, IDisposable
{
    private sealed class RunState(TestAgentRun snapshot)
    {
        public readonly object Gate = new();
        public readonly CancellationTokenSource Cancellation = new();
        public TestAgentRun Snapshot = snapshot;
    }

    private readonly TestAgentOptions _options = options.Value;
    private readonly ConcurrentDictionary<Guid, RunState> _runs = new();

    public TestAgentRun Start(StartTestAgentRunRequest request)
    {
        if (!_options.Enabled) throw new InvalidOperationException("The AI Test Agent experiment is not enabled.");
        if (string.IsNullOrWhiteSpace(_options.OpenAiApiKey)) throw new InvalidOperationException("The AI model API key is not configured.");
        var mission = request.Mission?.Trim();
        if (string.IsNullOrWhiteSpace(mission)) throw new ArgumentException("Describe what the AI agent should test.", nameof(request));
        if (mission.Length > 4000) throw new ArgumentException("The mission must be 4,000 characters or fewer.", nameof(request));

        var maximum = Math.Clamp(request.MaxActions ?? _options.MaximumActions, 1, _options.MaximumActions);
        var startUrl = string.IsNullOrWhiteSpace(request.StartUrl) ? _options.BackOfficeBaseUrl : request.StartUrl.Trim();
        if (!Uri.TryCreate(startUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new ArgumentException("Start URL must be an absolute HTTP or HTTPS address.", nameof(request));

        var now = DateTimeOffset.UtcNow;
        var run = new TestAgentRun(Guid.NewGuid(), mission, startUrl, "queued", maximum, 0, now, null, null, null, null,
            [new(now, "queued", "Mission accepted and waiting for the AI browser agent.")]);
        var state = new RunState(run);
        _runs[run.Id] = state;
        _ = Task.Run(() => ExecuteAsync(state), CancellationToken.None);
        return run;
    }

    public TestAgentRun? Get(Guid id) => _runs.TryGetValue(id, out var state) ? Read(state) : null;
    public IReadOnlyCollection<TestAgentRun> List() => _runs.Values.Select(Read).OrderByDescending(x => x.CreatedUtc).Take(50).ToArray();

    public bool Cancel(Guid id)
    {
        if (!_runs.TryGetValue(id, out var state)) return false;
        state.Cancellation.Cancel();
        return true;
    }

    private static TestAgentRun Read(RunState state) { lock (state.Gate) return state.Snapshot with { Events = state.Snapshot.Events.ToArray() }; }

    private void Update(RunState state, Func<TestAgentRun, TestAgentRun> update)
    {
        lock (state.Gate) state.Snapshot = update(state.Snapshot);
    }

    private void AddEvent(RunState state, string kind, string summary, string? screenshot = null)
    {
        Update(state, run => run with { Events = [.. run.Events, new(DateTimeOffset.UtcNow, kind, summary, screenshot)] });
    }

    private async Task ExecuteAsync(RunState state)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(_options.MaximumMinutes));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(state.Cancellation.Token, timeout.Token);
        var token = linked.Token;
        Update(state, run => run with { Status = "running", StartedUtc = DateTimeOffset.UtcNow });

        try
        {
            using var playwright = await Playwright.CreateAsync().ConfigureAwait(false);
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true }).ConfigureAwait(false);
            var page = await browser.NewPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 1000 } }).ConfigureAwait(false);
            await page.GotoAsync(state.Snapshot.StartUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30_000 }).ConfigureAwait(false);
            AddEvent(state, "browser", $"Opened {state.Snapshot.StartUrl}", await ScreenshotAsync(page).ConfigureAwait(false));

            for (var index = 0; index < state.Snapshot.MaxActions; index++)
            {
                token.ThrowIfCancellationRequested();
                var observation = await ObserveAsync(page).ConfigureAwait(false);
                var decision = await DecideAsync(state.Snapshot.Mission, state.Snapshot.Events, observation, token).ConfigureAwait(false);
                if (decision.Action.Equals("finish", StringComparison.OrdinalIgnoreCase))
                {
                    Update(state, run => run with { Status = "completed", CompletedUtc = DateTimeOffset.UtcNow, Assessment = decision.Assessment ?? decision.Summary ?? "The agent completed the mission." });
                    AddEvent(state, "assessment", decision.Summary ?? "Testing complete.", await ScreenshotAsync(page).ConfigureAwait(false));
                    return;
                }

                await ActAsync(page, decision, new Uri(state.Snapshot.StartUrl)).ConfigureAwait(false);
                Update(state, run => run with { ActionsCompleted = run.ActionsCompleted + 1 });
                AddEvent(state, "action", decision.Summary ?? $"AI chose {decision.Action}.", await ScreenshotAsync(page).ConfigureAwait(false));
            }

            Update(state, run => run with { Status = "completed", CompletedUtc = DateTimeOffset.UtcNow, Assessment = "The action limit was reached before the agent declared the mission complete." });
            AddEvent(state, "limit", "Stopped at the configured action limit.");
        }
        catch (OperationCanceledException)
        {
            var timedOut = timeout.IsCancellationRequested && !state.Cancellation.IsCancellationRequested;
            Update(state, run => run with { Status = timedOut ? "failed" : "cancelled", CompletedUtc = DateTimeOffset.UtcNow, Error = timedOut ? "The test exceeded its time limit." : null });
            AddEvent(state, timedOut ? "error" : "cancelled", timedOut ? "The run timed out." : "The run was cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "AI Test Agent run {RunId} failed", state.Snapshot.Id);
            Update(state, run => run with { Status = "failed", CompletedUtc = DateTimeOffset.UtcNow, Error = exception.Message });
            AddEvent(state, "error", exception.Message);
        }
    }

    private async Task<AgentDecision> DecideAsync(string mission, IReadOnlyList<TestAgentEvent> events, string observation, CancellationToken token)
    {
        var client = httpClientFactory.CreateClient(nameof(TestAgentRunService));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.OpenAiApiKey);
        var history = string.Join("\n", events.TakeLast(8).Select(x => $"{x.Kind}: {x.Summary}"));
        var prompt = $"""
You are an autonomous software tester operating Vennue through a real browser. Mission: {mission}
Choose exactly one next action. Prefer visible user workflows; do not use product APIs for the workflow. Investigate confusing behavior, errors, and reasonable variations. Never leave the supplied application origin. Recent events:\n{history}
Current page:\n{observation}
Return only JSON with: action (click|fill|navigate|wait|finish), selector (Playwright text or CSS selector), value, url, summary, assessment. On finish, assessment must state pass/fail, findings, and reproduction steps.
""";
        using var response = await client.PostAsJsonAsync("https://api.openai.com/v1/responses", new { model = _options.Model, input = prompt }, token).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(body);
        var output = document.RootElement.GetProperty("output")[0].GetProperty("content")[0].GetProperty("text").GetString() ?? "";
        output = output.Trim().Trim('`');
        if (output.StartsWith("json", StringComparison.OrdinalIgnoreCase)) output = output[4..].Trim();
        return JsonSerializer.Deserialize<AgentDecision>(output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("The AI model did not return a browser action.");
    }

    private static async Task<string> ObserveAsync(IPage page)
    {
        var title = await page.TitleAsync().ConfigureAwait(false);
        var text = await page.Locator("body").InnerTextAsync(new() { Timeout = 10_000 }).ConfigureAwait(false);
        return $"URL: {page.Url}\nTitle: {title}\nVisible text:\n{text[..Math.Min(text.Length, 12000)]}";
    }

    private static async Task ActAsync(IPage page, AgentDecision decision, Uri allowedOrigin)
    {
        switch (decision.Action.ToLowerInvariant())
        {
            case "click": await page.Locator(decision.Selector ?? throw new InvalidOperationException("Click requires a selector.")).First.ClickAsync(new() { Timeout = 10_000 }).ConfigureAwait(false); break;
            case "fill": await page.Locator(decision.Selector ?? throw new InvalidOperationException("Fill requires a selector.")).First.FillAsync(decision.Value ?? "", new() { Timeout = 10_000 }).ConfigureAwait(false); break;
            case "navigate":
                var target = new Uri(decision.Url ?? throw new InvalidOperationException("Navigate requires a URL."), UriKind.Absolute);
                if (!string.Equals(target.Scheme, allowedOrigin.Scheme, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(target.Authority, allowedOrigin.Authority, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The AI agent attempted to leave the configured application origin.");
                await page.GotoAsync(target.ToString(), new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30_000 }).ConfigureAwait(false);
                break;
            case "wait": await page.WaitForTimeoutAsync(1000).ConfigureAwait(false); break;
            default: throw new InvalidOperationException($"Unsupported AI browser action '{decision.Action}'.");
        }
    }

    private static async Task<string> ScreenshotAsync(IPage page) => Convert.ToBase64String(await page.ScreenshotAsync(new() { Type = ScreenshotType.Jpeg, Quality = 55 }).ConfigureAwait(false));

    public void Dispose() { foreach (var state in _runs.Values) state.Cancellation.Dispose(); }
}
