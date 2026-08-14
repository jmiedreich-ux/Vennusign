using System.Text;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Menus;

public sealed class MenuImportService(
    IMenuImportRepository imports,
    IContentRepository content,
    MenuBuilderConfigurationResolver configuration,
    MenuPasteParser parser,
    TimeProvider clock)
{
    private const int DefaultLineLimit = 2000;
    private const int DefaultRetentionMinutes = 1440;

    public async Task<MenuImportAggregate> StartAsync(Guid venueId, string rawPaste, string? actor, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawPaste)) throw new MenuImportValidationException("Paste at least one menu line to continue.");
        var now = clock.GetUtcNow().UtcDateTime;
        var resolved = await configuration.ResolveAsync(venueId, cancellationToken).ConfigureAwait(false);
        var bytes = Encoding.UTF8.GetByteCount(rawPaste);
        if (bytes > resolved.ImportFileSizeLimitBytes)
            throw new MenuImportValidationException($"That paste is {bytes:N0} bytes, over this venue's {resolved.ImportFileSizeLimitBytes:N0}-byte limit. Split it into two imports.");

        var ceilings = await content.GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var lineCount = CountLines(rawPaste);
        var lineLimit = ceilings.GetValueOrDefault(MenuCeilings.ImportLines, DefaultLineLimit);
        if (lineCount > lineLimit) throw new MenuImportValidationException(MenuCeilings.DescribeRefusal(MenuCeilings.ImportLines, lineCount, lineLimit));

        var id = Guid.NewGuid();
        var parsed = parser.Parse(id, venueId, rawPaste, 1, await content.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false));
        var itemLimit = ceilings.GetValueOrDefault(MenuCeilings.ItemsPerMenu, MenuCeilings.Defaults[MenuCeilings.ItemsPerMenu]);
        if (parsed.ItemCount > itemLimit)
            throw new MenuImportValidationException($"That paste contains {parsed.ItemCount:N0} items, over this venue's {itemLimit:N0}-item limit. Split it into two imports.");
        var retention = ceilings.GetValueOrDefault(MenuCeilings.ImportSessionRetentionMinutes, DefaultRetentionMinutes);
        var session = new MenuImportSession(id, venueId, rawPaste, 1, Status(parsed.Questions), lineCount, parsed.ItemCount,
            now.AddMinutes(retention), now, now, actor, []);
        _ = await imports.DeleteExpiredAsync(now, 100, cancellationToken).ConfigureAwait(false);
        return await imports.CreateAsync(new(session, parsed.Lines, parsed.Questions), cancellationToken).ConfigureAwait(false);
    }

    public Task<MenuImportAggregate?> GetAsync(Guid venueId, Guid sessionId, CancellationToken cancellationToken) =>
        imports.GetAsync(venueId, sessionId, clock.GetUtcNow().UtcDateTime, cancellationToken);

    public Task<MenuImportMutationOutcome> PutAnswerAsync(Guid venueId, Guid sessionId, byte[] revision, string questionKey,
        string fingerprint, string choice, Guid? selectedItemId, string? actor, CancellationToken cancellationToken) =>
        imports.PutAnswerAsync(venueId, sessionId, revision, questionKey, fingerprint, choice, selectedItemId,
            clock.GetUtcNow().UtcDateTime, actor, cancellationToken);

    public Task<MenuImportMutationOutcome> AcceptSafeMatchesAsync(Guid venueId, Guid sessionId, byte[] revision, string? actor,
        CancellationToken cancellationToken) => imports.AcceptSafeMatchesAsync(venueId, sessionId, revision,
            clock.GetUtcNow().UtcDateTime, actor, cancellationToken);

    public async Task<MenuImportMutationOutcome> SetSectionOverrideAsync(Guid venueId, Guid sessionId, byte[] revision, int lineNumber,
        bool isSection, string? actor, CancellationToken cancellationToken)
    {
        var current = await imports.GetAsync(venueId, sessionId, clock.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
        if (current is null) return new(MenuImportMutationOutcome.NotFound, null);
        if (lineNumber < 1 || current.Lines.All(line => line.LineNumber != lineNumber)) return new(MenuImportMutationOutcome.Invalid, null);
        var overrides = current.Lines.Where(line => line.Disposition == "section" && !IsNaturalHeading(line.RawText))
            .Select(line => line.LineNumber).ToHashSet();
        if (isSection) overrides.Add(lineNumber); else overrides.Remove(lineNumber);
        var nextRevision = current.Session.ParseRevision + 1;
        var parsed = parser.Parse(sessionId, venueId, current.Session.RawPaste, nextRevision,
            await content.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false), overrides);
        var now = clock.GetUtcNow().UtcDateTime;
        var nextSession = current.Session with { ParseRevision = nextRevision, Status = Status(parsed.Questions), LineCount = parsed.Lines.Count, ItemCount = parsed.ItemCount, UpdatedUtc = now, UpdatedBy = actor, Revision = [] };
        return await imports.ReplaceParseAsync(new(nextSession, parsed.Lines, parsed.Questions), revision, cancellationToken).ConfigureAwait(false);
    }

    public Task<int> DeleteExpiredAsync(int batchSize, CancellationToken cancellationToken) =>
        imports.DeleteExpiredAsync(clock.GetUtcNow().UtcDateTime, batchSize, cancellationToken);

    private static string Status(IReadOnlyCollection<MenuImportReviewQuestion> questions) => questions.Any(q => q.Required) ? MenuImportStatuses.Reviewing : MenuImportStatuses.Resolved;
    private static int CountLines(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n').Length;
    private static bool IsNaturalHeading(string value) { var trimmed = value.Trim(); return trimmed.Any(char.IsLetter) && trimmed.Where(char.IsLetter).All(char.IsUpper) && trimmed.Length <= Item.NameMaxLength; }
}

public sealed class MenuImportValidationException(string message) : Exception(message);
