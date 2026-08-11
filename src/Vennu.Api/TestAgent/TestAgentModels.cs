namespace Vennu.Api.TestAgent;

public sealed record StartTestAgentRunRequest(string Mission, string? StartUrl, int? MaxActions);

public sealed record TestAgentEvent(DateTimeOffset OccurredUtc, string Kind, string Summary, string? ScreenshotBase64 = null);

public sealed record TestAgentRun(
    Guid Id,
    string Mission,
    string StartUrl,
    string Status,
    int MaxActions,
    int ActionsCompleted,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? StartedUtc,
    DateTimeOffset? CompletedUtc,
    string? Assessment,
    string? Error,
    IReadOnlyList<TestAgentEvent> Events);

internal sealed record AgentDecision(string Action, string? Selector, string? Value, string? Url, string? Summary, string? Assessment);
