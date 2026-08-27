using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;

namespace Vennu.Api.Menus;

/// <summary>
/// What the rules could not place, suggested rather than asked.
///
/// The deterministic parser reads a real four-page menu down to two lines it cannot classify: the
/// restaurant's own name and its tagline, straddling a page break. No rule reaches them, because a
/// heading and a restaurant's name are the same shape. A language model reads them correctly and
/// says why, in one call, for under a cent.
///
/// **It sees the residue and the two lines either side of it, and nothing more.** Those neighbours
/// are the judgement - whether a line is a heading depends entirely on what follows it - so they go
/// with it. What matters is the other half of the guarantee: a verdict about any line that was not
/// asked about is discarded on the way back in. The model can inform a decision about the residue;
/// it cannot reach a line the rules already settled, which is what keeps the parse deterministic
/// and its tests meaningful.
///
/// **It suggests; it never answers.** A18 permits nothing to be pre-answered unless a rule can name
/// why, and a model names a reason rather than a rule. The verdict is stored beside the question
/// and applied only when the operator says so.
///
/// **It fails quietly.** No key, no network, a refusal, a malformed body: the import is exactly the
/// import it would have been without it. A convenience on two lines may not be able to break a
/// paste that otherwise works.
/// </summary>
public sealed class MenuResidueSuggestionService(
    HttpClient http,
    IOptions<MenuSuggestionOptions> options,
    ILogger<MenuResidueSuggestionService> logger)
{
    private readonly MenuSuggestionOptions settings = options.Value;

    /// <summary>More than this many unplaced lines means the parser is wrong, not the menu odd.</summary>
    private const int MaxResidueLines = 12;

    public bool Enabled => settings.Enabled && !string.IsNullOrWhiteSpace(settings.ApiKey);

    public async Task<MenuResidueSuggestion?> SuggestAsync(ParsedMenuPaste parsed, CancellationToken cancellationToken)
    {
        if (!Enabled) return null;

        var residue = parsed.Lines.Where(line => line.Disposition == "unresolved").ToArray();
        if (residue.Length is 0 or > MaxResidueLines) return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
            {
                Content = new StringContent(BuildRequest(parsed, residue), Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("x-api-key", settings.ApiKey);
            request.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));
            using var response = await http.SendAsync(request, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Menu residue suggestion declined with {Status}; the import continues without it.", response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
            return Read(body, residue.Select(line => line.LineNumber).ToHashSet());
        }
        catch (Exception failure) when (failure is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            // Deliberately swallowed. The import is the import either way.
            logger.LogWarning(failure, "Menu residue suggestion failed; the import continues without it.");
            return null;
        }
    }

    private string BuildRequest(ParsedMenuPaste parsed, IReadOnlyCollection<MenuImportSourceLine> residue)
    {
        var sections = parsed.Lines.Where(line => line.Disposition == "section").Select(line => line.ParsedName).ToArray();
        var context = new StringBuilder();
        context.Append("The parser read ").Append(parsed.ItemCount).Append(" items across these sections: ")
            .Append(sections.Length == 0 ? "(none)" : string.Join(", ", sections)).Append(".\n\n")
            .Append("It could not classify these lines. Each is shown with the lines around it.\n");

        /*
         * Grouped, not keyed. A line number stopped being unique in M6.9, when one pasted line
         * gained the ability to hold several items - "Sides: Jasmine Rice $2.00, Brown Rice $3.00"
         * is five rows sharing line 128. Keying a dictionary on it threw, the catch below swallowed
         * it as designed, and the suggestion silently never arrived on any menu with a line like
         * that. Failing quietly is right for a network call and wrong for a defect in this file;
         * the test that now covers it is the part that makes the difference.
         */
        var byNumber = parsed.Lines.GroupBy(line => line.LineNumber).ToDictionary(group => group.Key, group => group.First());
        foreach (var line in residue)
        {
            context.Append("\nLine ").Append(line.LineNumber).Append(":\n");
            for (var number = line.LineNumber - 2; number <= line.LineNumber + 2; number++)
            {
                if (!byNumber.TryGetValue(number, out var neighbour)) continue;
                var text = neighbour.RawText.Trim();
                context.Append("  ").Append(number).Append(": ").Append(text.Length == 0 ? "(blank)" : text).Append('\n');
            }
        }

        return JsonSerializer.Serialize(new
        {
            model = settings.Model,
            max_tokens = 1024,
            thinking = new { type = "adaptive" },
            output_config = new { effort = settings.Effort, format = new { type = "json_schema", schema = ResponseSchema } },
            system = SystemPrompt,
            messages = new[] { new { role = "user", content = context.ToString() } }
        });
    }

    private const string SystemPrompt =
        "You are helping a restaurant import a pasted menu into digital signage software. A deterministic " +
        "parser has already classified every line it could; you are shown only the lines it could not, and " +
        "you must not comment on any other line. For each line say what it is. If one is the restaurant's " +
        "or the menu's own name, say menu_name. If one is a tagline or strapline describing the menu, say " +
        "menu_description. Prefer leave_out over inventing a purpose for a line. State confidence honestly - " +
        "low is a useful answer and an operator will check every one of these.";

    private static readonly object ResponseSchema = JsonSerializer.Deserialize<JsonElement>("""
    {
      "type": "object",
      "additionalProperties": false,
      "required": ["menuName", "menuDescription", "lines"],
      "properties": {
        "menuName": { "type": ["string", "null"] },
        "menuDescription": { "type": ["string", "null"] },
        "lines": {
          "type": "array",
          "items": {
            "type": "object",
            "additionalProperties": false,
            "required": ["lineNumber", "verdict", "confidence", "why"],
            "properties": {
              "lineNumber": { "type": "integer" },
              "verdict": { "type": "string", "enum": ["menu_name", "menu_description", "section_heading", "dish", "leave_out"] },
              "confidence": { "type": "string", "enum": ["high", "medium", "low"] },
              "why": { "type": "string" }
            }
          }
        }
      }
    }
    """);

    /// <summary>
    /// Reads the reply, and discards anything about a line that was not sent.
    ///
    /// The schema constrains the shape; it cannot constrain the line numbers. A verdict about a line
    /// the rules already settled is dropped here rather than trusted, because the guarantee that the
    /// model only touches the residue has to be enforced on the way back in, not just asked for on
    /// the way out.
    /// </summary>
    private MenuResidueSuggestion? Read(string body, IReadOnlySet<int> asked)
    {
        var payload = JsonSerializer.Deserialize<AnthropicResponse>(body);
        var text = payload?.Content?.FirstOrDefault(block => block.Type == "text")?.Text;
        if (string.IsNullOrWhiteSpace(text)) return null;

        var verdicts = JsonSerializer.Deserialize<SuggestionPayload>(text, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (verdicts is null) return null;

        var lines = (verdicts.Lines ?? [])
            .Where(line => asked.Contains(line.LineNumber))
            .Where(line => !string.IsNullOrWhiteSpace(line.Verdict))
            .Select(line => new MenuResidueLineSuggestion(line.LineNumber, line.Verdict!, Describe(line)))
            .ToArray();

        return lines.Length == 0 && verdicts.MenuName is null ? null
            : new MenuResidueSuggestion(Trim(verdicts.MenuName, 200), Trim(verdicts.MenuDescription, 500), lines);
    }

    private static string Describe(SuggestionLine line) =>
        string.IsNullOrWhiteSpace(line.Why) ? string.Empty : Trim($"{line.Why} ({line.Confidence ?? "unstated"} confidence)", 300)!;

    private static string? Trim(string? value, int limit) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Length <= limit ? value : value[..limit];

    private sealed record AnthropicResponse([property: JsonPropertyName("content")] ContentBlock[]? Content);
    private sealed record ContentBlock([property: JsonPropertyName("type")] string? Type, [property: JsonPropertyName("text")] string? Text);
    private sealed record SuggestionPayload(string? MenuName, string? MenuDescription, SuggestionLine[]? Lines);
    private sealed record SuggestionLine(int LineNumber, string? Verdict, string? Confidence, string? Why);
}

public sealed record MenuResidueSuggestion(string? MenuName, string? MenuDescription, IReadOnlyCollection<MenuResidueLineSuggestion> Lines);

public sealed record MenuResidueLineSuggestion(int LineNumber, string Verdict, string Reason);

/// <summary>
/// Configuration for the residue pass. Off unless a key is present, so every environment without
/// one behaves exactly as it did before this shipped.
/// </summary>
public sealed class MenuSuggestionOptions
{
    public const string Section = "MenuImport:Anthropic";

    public bool Enabled { get; set; } = true;

    public string? ApiKey { get; set; }

    /// <summary>Opus 5: the whole point of this call is the judgement a rule cannot make.</summary>
    public string Model { get; set; } = "claude-opus-5";

    /// <summary>A short classification. Thinking stays on; its depth does not need to be.</summary>
    public string Effort { get; set; } = "low";

    public string BaseUrl { get; set; } = "https://api.anthropic.com/";

    /// <summary>An operator is waiting on this. It is a convenience, not a reason to hold the screen.</summary>
    public int TimeoutSeconds { get; set; } = 20;
}
