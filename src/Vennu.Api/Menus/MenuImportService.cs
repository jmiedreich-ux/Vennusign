using System.Text;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Menus;

public sealed class MenuImportService(
    IMenuImportRepository imports,
    IContentRepository content,
    MenuBuilderConfigurationResolver configuration,
    MenuPasteParser parser,
    MenuResidueSuggestionService suggestions,
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
        var parsed = parser.Parse(id, venueId, rawPaste, 1, await content.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false),
            dependencyStamp: DependencyStamp(resolved, ceilings));
        var itemLimit = ceilings.GetValueOrDefault(MenuCeilings.ItemsPerMenu, MenuCeilings.Defaults[MenuCeilings.ItemsPerMenu]);
        if (parsed.ItemCount > itemLimit)
            throw new MenuImportValidationException($"That paste contains {parsed.ItemCount:N0} items, over this venue's {itemLimit:N0}-item limit. Split it into two imports.");
        var retention = ceilings.GetValueOrDefault(MenuCeilings.ImportSessionRetentionMinutes, DefaultRetentionMinutes);
        /*
         * The rules have finished. Whatever they could not place is offered to the residue pass,
         * which suggests and never answers (A18) - so the session below is the same session either
         * way, carrying a suggestion the operator may apply or ignore. It fails quietly: a
         * convenience on a handful of lines may not be able to break a paste that otherwise works.
         */
        var suggestion = await suggestions.SuggestAsync(parsed, cancellationToken).ConfigureAwait(false);
        var lines = Suggested(parsed.Lines, suggestion);
        var questions = await DistinguishedAsync(venueId, parsed.Questions, cancellationToken).ConfigureAwait(false);
        var session = new MenuImportSession(id, venueId, rawPaste, 1, Status(parsed.Questions), lineCount, parsed.ItemCount,
            now.AddMinutes(retention), now, now, actor, [],
            SuggestedMenuName: suggestion?.MenuName, SuggestedMenuDescription: suggestion?.MenuDescription);
        _ = await imports.DeleteExpiredAsync(now, 100, cancellationToken).ConfigureAwait(false);
        return await imports.CreateAsync(new(session, lines, questions), cancellationToken).ConfigureAwait(false);
    }

    public Task<MenuImportAggregate?> GetAsync(Guid venueId, Guid sessionId, CancellationToken cancellationToken) =>
        RefreshDependenciesAsync(venueId, sessionId, cancellationToken);

    /// <summary>
    /// The venue's unfinished imports, so a screen can offer the way back to one.
    ///
    /// The expiry sweep runs first. Otherwise the list is the one place guaranteed to report an
    /// import that has already expired - it is read far more often than a session is started.
    /// </summary>
    public async Task<IReadOnlyCollection<MenuImportSummary>> ListOpenAsync(Guid venueId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        _ = await imports.DeleteExpiredAsync(now, 100, cancellationToken).ConfigureAwait(false);
        return await imports.ListOpenAsync(venueId, now, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Throws away an unfinished import at the operator's word, rather than waiting out its expiry.</summary>
    public Task<bool> DiscardAsync(Guid venueId, Guid sessionId, CancellationToken cancellationToken) =>
        imports.DiscardAsync(venueId, sessionId, cancellationToken);

    public async Task<MenuImportMutationOutcome> PutAnswerAsync(Guid venueId, Guid sessionId, byte[] revision, string questionKey,
        string fingerprint, string choice, Guid? selectedItemId, string? actor, CancellationToken cancellationToken)
    {
        var current = await RefreshDependenciesAsync(venueId, sessionId, cancellationToken).ConfigureAwait(false);
        if (current is null) return new(MenuImportMutationOutcome.NotFound, null);
        if (!current.Session.Revision.SequenceEqual(revision)) return new(MenuImportMutationOutcome.Conflict, current);
        return await imports.PutAnswerAsync(venueId, sessionId, revision, questionKey, fingerprint, choice, selectedItemId,
            clock.GetUtcNow().UtcDateTime, actor, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MenuImportMutationOutcome> AcceptSafeMatchesAsync(Guid venueId, Guid sessionId, byte[] revision, string? actor,
        CancellationToken cancellationToken)
    {
        var current = await RefreshDependenciesAsync(venueId, sessionId, cancellationToken).ConfigureAwait(false);
        if (current is null) return new(MenuImportMutationOutcome.NotFound, null);
        if (!current.Session.Revision.SequenceEqual(revision)) return new(MenuImportMutationOutcome.Conflict, current);
        return await imports.AcceptSafeMatchesAsync(venueId, sessionId, revision,
            clock.GetUtcNow().UtcDateTime, actor, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MenuImportMutationOutcome> SetSectionOverrideAsync(Guid venueId, Guid sessionId, byte[] revision, int lineNumber,
        bool isSection, string? actor, CancellationToken cancellationToken)
    {
        var current = await RefreshDependenciesAsync(venueId, sessionId, cancellationToken).ConfigureAwait(false);
        if (current is null) return new(MenuImportMutationOutcome.NotFound, null);
        if (!current.Session.Revision.SequenceEqual(revision)) return new(MenuImportMutationOutcome.Conflict, current);
        if (lineNumber < 1 || current.Lines.All(line => line.LineNumber != lineNumber)) return new(MenuImportMutationOutcome.Invalid, null);
        var overrides = current.Lines.Where(line => line.Disposition == "section" && !IsNaturalHeading(line.RawText))
            .Select(line => line.LineNumber).ToHashSet();
        if (isSection) overrides.Add(lineNumber); else overrides.Remove(lineNumber);
        var nextRevision = current.Session.ParseRevision + 1;
        var parsed = parser.Parse(sessionId, venueId, current.Session.RawPaste, nextRevision,
            await content.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false), overrides,
            DependencyStamp(await configuration.ResolveAsync(venueId, cancellationToken).ConfigureAwait(false),
                await content.GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false)));
        var now = clock.GetUtcNow().UtcDateTime;
        var nextSession = current.Session with { ParseRevision = nextRevision, Status = Status(parsed.Questions), LineCount = parsed.Lines.Count, ItemCount = parsed.ItemCount, UpdatedUtc = now, UpdatedBy = actor, Revision = [] };
        return await imports.ReplaceParseAsync(
            new(nextSession, CarriedForward(current.Lines, parsed.Lines),
                await DistinguishedAsync(venueId, parsed.Questions, cancellationToken).ConfigureAwait(false)),
            revision, cancellationToken).ConfigureAwait(false);
    }

    public Task<int> DeleteExpiredAsync(int batchSize, CancellationToken cancellationToken) =>
        imports.DeleteExpiredAsync(clock.GetUtcNow().UtcDateTime, batchSize, cancellationToken);

    public async Task<MenuImportMutationOutcome> SetCreateDestinationAsync(Guid venueId, Guid sessionId, byte[] revision,
        string menuName, string? actor, CancellationToken cancellationToken)
    {
        var current = await RefreshDependenciesAsync(venueId, sessionId, cancellationToken).ConfigureAwait(false);
        if (current is null) return new(MenuImportMutationOutcome.NotFound, null);
        if (!current.Session.Revision.SequenceEqual(revision)) return new(MenuImportMutationOutcome.Conflict, current);
        return await imports.SetCreateDestinationAsync(venueId, sessionId, revision, menuName,
            clock.GetUtcNow().UtcDateTime, actor, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MenuImportCreateOutcome> ConfirmCreateAsync(Guid venueId, Guid sessionId, byte[] revision,
        Guid actorUserId, IReadOnlyCollection<string> systemRoleKeys, string? actor, CancellationToken cancellationToken)
    {
        var current = await RefreshDependenciesAsync(venueId, sessionId, cancellationToken).ConfigureAwait(false);
        if (current is null) return new(MenuImportMutationOutcome.NotFound, null, null);
        if (current.Session.CompletedMenuId is null && !current.Session.Revision.SequenceEqual(revision))
            return new(MenuImportMutationOutcome.Conflict, current, null);
        return await imports.ConfirmCreateAsync(venueId, sessionId, revision, actorUserId, systemRoleKeys,
            clock.GetUtcNow().UtcDateTime, actor, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MenuImportReplaceDestinationOutcome> SetReplaceDestinationAsync(Guid venueId,Guid sessionId,byte[] revision,
        Guid menuId,string? actor,CancellationToken cancellationToken)
    {
        var current=await RefreshDependenciesAsync(venueId,sessionId,cancellationToken).ConfigureAwait(false);
        if(current is null)return new(MenuImportMutationOutcome.NotFound,null,null);
        if(!current.Session.Revision.SequenceEqual(revision))return new(MenuImportMutationOutcome.Conflict,current,null);
        // Choosing the target IS the moment the confirm screen first draws, so the preview has to
        // travel with this answer rather than waiting for a read that may never happen.
        var chosen=await imports.SetReplaceDestinationAsync(venueId,sessionId,revision,menuId,clock.GetUtcNow().UtcDateTime,actor,cancellationToken).ConfigureAwait(false);
        return chosen with { Aggregate=await WithReplacePreviewAsync(chosen.Aggregate,venueId,cancellationToken).ConfigureAwait(false) };
    }

    public async Task<MenuImportCreateOutcome> ConfirmReplaceAsync(Guid venueId,Guid sessionId,byte[] revision,Guid actorUserId,
        IReadOnlyCollection<string> systemRoleKeys,string? actor,CancellationToken cancellationToken)
    {
        var current=await RefreshDependenciesAsync(venueId,sessionId,cancellationToken).ConfigureAwait(false);
        if(current is null)return new(MenuImportMutationOutcome.NotFound,null,null);
        if(current.Session.CompletedMenuId is null&&!current.Session.Revision.SequenceEqual(revision))return new(MenuImportMutationOutcome.Conflict,current,null);
        var outcome=await imports.ConfirmReplaceAsync(venueId,sessionId,revision,actorUserId,systemRoleKeys,clock.GetUtcNow().UtcDateTime,actor,cancellationToken).ConfigureAwait(false);
        if(outcome.Result=="target_conflict"&&outcome.Aggregate?.Session.TargetMenuId is Guid targetMenuId)
        {
            // The target moved under the operator, so the facts are recomputed before they are shown
            // again - including the preview, which is the whole point of a conflict: what you were
            // about to do is not what would have happened.
            var refreshed=await imports.SetReplaceDestinationAsync(venueId,sessionId,outcome.Aggregate.Session.Revision,targetMenuId,clock.GetUtcNow().UtcDateTime,actor,cancellationToken).ConfigureAwait(false);
            if(refreshed.Result==MenuImportMutationOutcome.Updated)
                return outcome with { Aggregate=await WithReplacePreviewAsync(refreshed.Aggregate,venueId,cancellationToken).ConfigureAwait(false) };
        }
        return outcome;
    }

    public Task<MenuImportRestoreOutcome> RestoreReplacementAsync(Guid venueId,Guid snapshotId,Guid actorUserId,
        IReadOnlyCollection<string> systemRoleKeys,string? actor,CancellationToken cancellationToken)=>
        imports.RestoreReplacementAsync(venueId,snapshotId,actorUserId,systemRoleKeys,clock.GetUtcNow().UtcDateTime,actor,cancellationToken);

    /// <summary>
    /// Attaches what replacing the chosen menu would do (M6.13).
    ///
    /// Computed on every read and never stored. The counts stored beside it describe the TARGET
    /// menu, which is safe because the target has a lease - TargetWorkingFingerprint, checked at
    /// confirm. This depends on the SESSION's lines and answers, which have no lease: answering a
    /// question, promoting a line, or merely reading the session can re-parse and rebuild every
    /// answer. Storing it would not buy a stable explanation, it would buy a stable wrong one -
    /// and on a conflict the screen re-renders exactly that, so the operator reads one thing and
    /// presses a button that does another.
    ///
    /// Nothing is previewed for a completed session: the replacement already happened, and a
    /// preview of it would describe a decision nobody is making any more.
    /// </summary>
    private async Task<MenuImportAggregate?> WithReplacePreviewAsync(
        MenuImportAggregate? aggregate, Guid venueId, CancellationToken cancellationToken)
    {
        if (aggregate is null
            || aggregate.Session.CompletedMenuId is not null
            || aggregate.Session.Destination != MenuImportDestinations.Replace
            || aggregate.Session.TargetMenuId is null)
        {
            return aggregate;
        }

        var placements = await content.GetPlacementsAsync(venueId, aggregate.Session.TargetMenuId.Value, cancellationToken).ConfigureAwait(false);
        var library = await content.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false);
        return aggregate with { ReplacePreview = MenuImportReplacePreviewBuilder.Build(aggregate, placements, library) };
    }

    private async Task<MenuImportAggregate?> RefreshDependenciesAsync(Guid venueId, Guid sessionId, CancellationToken cancellationToken)
    {
        var refreshed = await RefreshedAsync(venueId, sessionId, cancellationToken).ConfigureAwait(false);
        return await WithReplacePreviewAsync(refreshed, venueId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<MenuImportAggregate?> RefreshedAsync(Guid venueId, Guid sessionId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var current = await imports.GetAsync(venueId, sessionId, now, cancellationToken).ConfigureAwait(false);
        if (current is null) return null;
        if (current.Session.CompletedMenuId is not null) return current;
        var overrides = current.Lines.Where(line => line.Disposition == "section" && !IsNaturalHeading(line.RawText))
            .Select(line => line.LineNumber).ToHashSet();
        var resolved = await configuration.ResolveAsync(venueId, cancellationToken).ConfigureAwait(false);
        var ceilings = await content.GetCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var lineLimit = ceilings.GetValueOrDefault(MenuCeilings.ImportLines, DefaultLineLimit);
        var itemLimit = ceilings.GetValueOrDefault(MenuCeilings.ItemsPerMenu, MenuCeilings.Defaults[MenuCeilings.ItemsPerMenu]);
        if (current.Session.LineCount > lineLimit || current.Session.ItemCount > itemLimit || Encoding.UTF8.GetByteCount(current.Session.RawPaste) > resolved.ImportFileSizeLimitBytes)
            throw new MenuImportValidationException("This saved import no longer fits the venue's current import limits. Paste a smaller menu to continue.");
        var parsed = parser.Parse(sessionId, venueId, current.Session.RawPaste, current.Session.ParseRevision + 1,
            await content.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false), overrides, DependencyStamp(resolved, ceilings));
        if (SameDependencies(current, parsed)) return current;

        var next = current.Session with
        {
            ParseRevision = current.Session.ParseRevision + 1,
            Status = Status(parsed.Questions),
            LineCount = parsed.Lines.Count,
            ItemCount = parsed.ItemCount,
            UpdatedUtc = now,
            Revision = []
        };
        var replaced = await imports.ReplaceParseAsync(
            new(next, CarriedForward(current.Lines, parsed.Lines),
                await DistinguishedAsync(venueId, parsed.Questions, cancellationToken).ConfigureAwait(false)),
            current.Session.Revision, cancellationToken).ConfigureAwait(false);
        return replaced.Aggregate;
    }

    private static bool SameDependencies(MenuImportAggregate current, ParsedMenuPaste parsed) =>
        current.Lines.Select(line => (line.LineNumber, line.Disposition, line.ParsedName, line.ParsedPrice, line.ParserReason))
            .SequenceEqual(parsed.Lines.Select(line => (line.LineNumber, line.Disposition, line.ParsedName, line.ParsedPrice, line.ParserReason))) &&
        current.Questions.Select(QuestionShape).SequenceEqual(parsed.Questions.Select(QuestionShape));

    private static string QuestionShape(MenuImportReviewQuestion question) => string.Join('|',
        question.QuestionKey, question.Fingerprint, question.Kind, string.Join(',', question.LineNumbers),
        string.Join(';', question.Candidates.Select(candidate => $"{candidate.ItemId}:{candidate.DisplayName}:{candidate.DisplayPrice}:{candidate.MatchRule}:{candidate.IsSafe}")));

    private static string DependencyStamp(ResolvedMenuBuilderConfiguration resolved, IReadOnlyDictionary<string, int> ceilings) =>
        string.Join('|', resolved.ImportFileSizeLimitBytes,
            ceilings.GetValueOrDefault(MenuCeilings.ImportLines, DefaultLineLimit),
            ceilings.GetValueOrDefault(MenuCeilings.ItemsPerMenu, MenuCeilings.Defaults[MenuCeilings.ItemsPerMenu]),
            ceilings.GetValueOrDefault(MenuCeilings.ImportSessionRetentionMinutes, DefaultRetentionMinutes));

    /*
     * A re-parse must not forget what the residue pass said.
     *
     * Every refresh writes the parser's own output straight back, and the parser knows nothing
     * about suggestions - so the first time anything changed underneath a session, the per-line
     * verdicts were wiped while the session-level name survived. The banner still offered to fill
     * the name in, the click found no verdict on any line, answered nothing, and the screen flashed
     * and sat there. Reported as "Yes, use these does nothing", and it did nothing.
     *
     * The raw paste has not changed, so a verdict about line 68 is still about line 68. It is
     * carried onto the new line only where that line is still unresolved and still says the same
     * thing; anything the re-parse has since placed or altered drops its verdict, because a
     * suggestion about a line that no longer exists in that form is not a suggestion, it is a
     * stale claim.
     */
    private static IReadOnlyCollection<MenuImportSourceLine> CarriedForward(
        IReadOnlyCollection<MenuImportSourceLine> previous, IReadOnlyCollection<MenuImportSourceLine> parsed)
    {
        var kept = previous.Where(line => line.SuggestedVerdict is not null)
            .GroupBy(line => line.LineNumber)
            .ToDictionary(group => group.Key, group => group.First());
        if (kept.Count == 0) return parsed;

        return parsed.Select(line => kept.TryGetValue(line.LineNumber, out var was)
            && line.Disposition == "unresolved"
            && string.Equals(line.RawText.Trim(), was.RawText.Trim(), StringComparison.Ordinal)
                ? line with { SuggestedVerdict = was.SuggestedVerdict, SuggestedReason = was.SuggestedReason }
                : line).ToArray();
    }

    /// <summary>
    /// A21 - where a question offers more than one candidate, says what tells them apart.
    ///
    /// A venue library can hold the same dish twice at the same price, split by an older import.
    /// Both rendered as "Use the one you already have - Pad Thai $12.95", identically, and an
    /// operator cannot answer that; they can only guess. The owner ruled against merging them
    /// silently on 2026-08-27: the screen names which menus each one is on and when it was made.
    ///
    /// Only ambiguous questions are enriched, and the read happens only if there are any. A
    /// question with one candidate has nothing to be distinguished from, and every import paying
    /// for a query whose answer no screen draws is work for its own sake.
    /// </summary>
    private async Task<IReadOnlyCollection<MenuImportReviewQuestion>> DistinguishedAsync(
        Guid venueId,
        IReadOnlyCollection<MenuImportReviewQuestion> questions,
        CancellationToken cancellationToken)
    {
        var ambiguous = questions.Where(question => question.Candidates.Count > 1).ToArray();
        if (ambiguous.Length == 0) return questions;

        var itemIds = ambiguous.SelectMany(question => question.Candidates.Select(candidate => candidate.ItemId)).Distinct().ToArray();
        var boards = await content.GetItemBoardsAsync(venueId, itemIds, cancellationToken).ConfigureAwait(false);
        var items = await content.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false);

        var menusByItem = boards
            .GroupBy(board => board.ItemId)
            .ToDictionary(group => group.Key, group => group.Select(board => board.MenuName).Distinct().OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase).ToArray());
        var createdByItem = items.ToDictionary(item => item.Id, item => item.CreatedUtc);

        return questions
            .Select(question => question.Candidates.Count > 1
                ? question with
                {
                    Candidates = question.Candidates
                        .Select(candidate => candidate with
                        {
                            // An empty list is not the same as "we did not look": the dish is on no
                            // menu, which is itself the thing that tells it from the one that is.
                            OnMenus = menusByItem.TryGetValue(candidate.ItemId, out var names) ? names : [],
                            ItemCreatedUtc = createdByItem.TryGetValue(candidate.ItemId, out var created) ? created : null
                        })
                        .ToArray()
                }
                : question)
            .ToArray();
    }

    /// <summary>Puts each verdict on the line it is about, so the screen can show it where the question is.</summary>
    private static IReadOnlyCollection<MenuImportSourceLine> Suggested(IReadOnlyCollection<MenuImportSourceLine> lines, MenuResidueSuggestion? suggestion)
    {
        if (suggestion is null || suggestion.Lines.Count == 0) return lines;
        var byLine = suggestion.Lines.GroupBy(line => line.LineNumber).ToDictionary(group => group.Key, group => group.First());
        return lines.Select(line => byLine.TryGetValue(line.LineNumber, out var verdict) && line.Disposition == "unresolved"
            ? line with { SuggestedVerdict = verdict.Verdict, SuggestedReason = verdict.Reason }
            : line).ToArray();
    }

    private static string Status(IReadOnlyCollection<MenuImportReviewQuestion> questions) => questions.Any(q => q.Required) ? MenuImportStatuses.Reviewing : MenuImportStatuses.Resolved;
    private static int CountLines(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n').Length;
    // One definition, in the parser. This used to be a byte-identical copy, and it was about
    // to disagree with the original: M6.7 made Title Case headings natural too.
    private static bool IsNaturalHeading(string value) => MenuPasteParser.IsNaturalHeading(value);
}

public sealed class MenuImportValidationException(string message) : Exception(message);
