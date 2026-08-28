using System.Text.Json;
using Microsoft.Data.SqlClient;
using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class MenuImportRepository(ISqlDataAccess dataAccess) : IMenuImportRepository
{
    public async Task<MenuImportAggregate> CreateAsync(MenuImportAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ValidateAggregate(aggregate);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                _ = (await dataAccess.ExecuteSqlQueryAsync<ResultRow, object>(CreateSql, Parameters(aggregate), cancellationToken)
                    .ConfigureAwait(false)).Single();
                break;
            }
            catch (SqlException exception) when (exception.Number == 1205 && attempt < 3)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(40 * attempt), cancellationToken).ConfigureAwait(false);
            }
        }
        return await GetAsync(aggregate.Session.VenueId, aggregate.Session.Id, aggregate.Session.CreatedUtc, cancellationToken)
            .ConfigureAwait(false) ?? throw new InvalidOperationException("The import session was not readable after creation.");
    }

    public async Task<MenuImportAggregate?> GetAsync(Guid venueId, Guid sessionId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        RequireId(sessionId, nameof(sessionId));
        var row = (await dataAccess.ExecuteSqlQueryAsync<AggregateRow, object>(ReadSql, new { VenueId = venueId, SessionId = sessionId, Now = nowUtc }, cancellationToken)
            .ConfigureAwait(false)).SingleOrDefault();
        return row is null ? null : Hydrate(row);
    }

    public async Task<IReadOnlyCollection<MenuImportSummary>> ListOpenAsync(
        Guid venueId, DateTime nowUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<SummaryRow, object>(
            ListOpenSql, new { VenueId = RequireId(venueId, nameof(venueId)), Now = nowUtc }, cancellationToken)
            .ConfigureAwait(false))
        .Select(row => new MenuImportSummary(
            row.Id, row.ItemCount, row.LineCount, row.AnswersRemaining, row.CreatedUtc, row.UpdatedUtc, row.ExpiresUtc))
        .ToArray();

    public async Task<bool> DiscardAsync(Guid venueId, Guid sessionId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(
            DiscardSql,
            new { VenueId = RequireId(venueId, nameof(venueId)), SessionId = RequireId(sessionId, nameof(sessionId)) },
            cancellationToken).ConfigureAwait(false)).Single().Count > 0;

    public Task<MenuImportMutationOutcome> PutAnswerAsync(Guid venueId, Guid sessionId, byte[] expectedRevision, string questionKey,
        string fingerprint, string choice, Guid? selectedItemId, DateTime answeredUtc, string? answeredBy,
        CancellationToken cancellationToken = default) => MutateAsync(AnswerSql, new
        {
            VenueId = RequireId(venueId, nameof(venueId)), SessionId = RequireId(sessionId, nameof(sessionId)),
            ExpectedRevision = RequireRevision(expectedRevision), QuestionKey = RequireText(questionKey, nameof(questionKey)),
            Fingerprint = RequireFingerprint(fingerprint), Choice = RequireAnswerShape(choice, selectedItemId), SelectedItemId = selectedItemId,
            Now = answeredUtc, Actor = answeredBy
        }, cancellationToken);

    public Task<MenuImportMutationOutcome> AcceptSafeMatchesAsync(Guid venueId, Guid sessionId, byte[] expectedRevision,
        DateTime answeredUtc, string? answeredBy, CancellationToken cancellationToken = default) => MutateAsync(AcceptSafeSql, new
        {
            VenueId = RequireId(venueId, nameof(venueId)), SessionId = RequireId(sessionId, nameof(sessionId)),
            ExpectedRevision = RequireRevision(expectedRevision), Now = answeredUtc, Actor = answeredBy
        }, cancellationToken);

    public async Task<MenuImportMutationOutcome> ReplaceParseAsync(MenuImportAggregate aggregate, byte[] expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ValidateAggregate(aggregate);
        var parameters = Parameters(aggregate, RequireRevision(expectedRevision));
        return await MutateAsync(ReplaceSql, parameters, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> DeleteExpiredAsync(DateTime nowUtc, int batchSize, CancellationToken cancellationToken = default)
    {
        if (batchSize is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(batchSize));
        return (await dataAccess.ExecuteSqlQueryAsync<CountRow, object>(DeleteExpiredSql, new { Now = nowUtc, BatchSize = batchSize }, cancellationToken)
            .ConfigureAwait(false)).Single().Count;
    }

    public Task<MenuImportMutationOutcome> SetCreateDestinationAsync(Guid venueId, Guid sessionId, byte[] expectedRevision,
        string menuName, DateTime nowUtc, string? actor, CancellationToken cancellationToken = default) =>
        MutateAsync(SetCreateDestinationSql, new
        {
            VenueId = RequireId(venueId, nameof(venueId)), SessionId = RequireId(sessionId, nameof(sessionId)),
            ExpectedRevision = RequireRevision(expectedRevision), MenuName = RequireMenuName(menuName), Now = nowUtc, Actor = actor
        }, cancellationToken);

    public async Task<MenuImportCreateOutcome> ConfirmCreateAsync(Guid venueId, Guid sessionId, byte[] expectedRevision,
        Guid actorUserId, IReadOnlyCollection<string> systemRoleKeys, DateTime nowUtc, string? actor,
        CancellationToken cancellationToken = default)
    {
        var result = (await dataAccess.ExecuteSqlQueryAsync<CreateResultRow, object>(ConfirmCreateSql, new
        {
            VenueId = RequireId(venueId, nameof(venueId)), SessionId = RequireId(sessionId, nameof(sessionId)),
            ExpectedRevision = RequireRevision(expectedRevision), ActorUserId = RequireId(actorUserId, nameof(actorUserId)),
            SystemRolesJson = JsonSerializer.Serialize(systemRoleKeys ?? throw new ArgumentNullException(nameof(systemRoleKeys))),
            DefaultMenuLimit = MenuCeilings.Defaults[MenuCeilings.MenusPerVenue],
            DefaultItemLimit = MenuCeilings.Defaults[MenuCeilings.ItemsPerMenu],
            Now = nowUtc, Actor = actor, MenuId = Guid.NewGuid(), PageId = Guid.NewGuid()
        }, cancellationToken).ConfigureAwait(false)).Single();
        var aggregate = result.Result is MenuImportMutationOutcome.NotFound or MenuImportMutationOutcome.Expired
            ? null : await GetAsync(venueId, sessionId, nowUtc, cancellationToken).ConfigureAwait(false);
        return new(result.Result, aggregate, result.MenuId);
    }

    public async Task<MenuImportReplaceDestinationOutcome> SetReplaceDestinationAsync(Guid venueId, Guid sessionId,
        byte[] expectedRevision, Guid menuId, DateTime nowUtc, string? actor, CancellationToken cancellationToken = default)
    {
        var row=(await dataAccess.ExecuteSqlQueryAsync<ReplaceDestinationRow,object>(SetReplaceDestinationSql,new {
            VenueId=RequireId(venueId,nameof(venueId)),SessionId=RequireId(sessionId,nameof(sessionId)),
            ExpectedRevision=RequireRevision(expectedRevision),MenuId=RequireId(menuId,nameof(menuId)),Now=nowUtc,Actor=actor
        },cancellationToken).ConfigureAwait(false)).Single();
        var aggregate=row.Result is MenuImportMutationOutcome.NotFound or MenuImportMutationOutcome.Expired ? null :
            await GetAsync(venueId,sessionId,nowUtc,cancellationToken).ConfigureAwait(false);
        return new(row.Result,aggregate,row.MenuId is null?null:new(row.MenuId.Value,row.MenuName!,row.TargetUpdatedUtc!.Value,
            row.HasPublishedVersion,row.WorkingItemCount,row.PublishedItemCount,row.Added,row.Removed,row.Changed));
    }

    public async Task<MenuImportCreateOutcome> ConfirmReplaceAsync(Guid venueId, Guid sessionId, byte[] expectedRevision,
        Guid actorUserId, IReadOnlyCollection<string> systemRoleKeys, DateTime nowUtc, string? actor,
        CancellationToken cancellationToken = default)
    {
        var row=(await dataAccess.ExecuteSqlQueryAsync<CreateResultRow,object>(ConfirmReplaceSql,new {
            VenueId=RequireId(venueId,nameof(venueId)),SessionId=RequireId(sessionId,nameof(sessionId)),
            ExpectedRevision=RequireRevision(expectedRevision),ActorUserId=RequireId(actorUserId,nameof(actorUserId)),
            SystemRolesJson=JsonSerializer.Serialize(systemRoleKeys),DefaultItemLimit=MenuCeilings.Defaults[MenuCeilings.ItemsPerMenu],
            Now=nowUtc,Actor=actor,SnapshotId=Guid.NewGuid()
        },cancellationToken).ConfigureAwait(false)).Single();
        var aggregate=row.Result is MenuImportMutationOutcome.NotFound or MenuImportMutationOutcome.Expired?null:
            await GetAsync(venueId,sessionId,nowUtc,cancellationToken).ConfigureAwait(false);
        return new(row.Result,aggregate,row.MenuId);
    }

    public async Task<MenuImportRestoreOutcome> RestoreReplacementAsync(Guid venueId,Guid snapshotId,Guid actorUserId,
        IReadOnlyCollection<string> systemRoleKeys,DateTime nowUtc,string? actor,CancellationToken cancellationToken=default)
    {
        var row=(await dataAccess.ExecuteSqlQueryAsync<CreateResultRow,object>(RestoreReplacementSql,new {
            VenueId=RequireId(venueId,nameof(venueId)),SnapshotId=RequireId(snapshotId,nameof(snapshotId)),
            ActorUserId=RequireId(actorUserId,nameof(actorUserId)),SystemRolesJson=JsonSerializer.Serialize(systemRoleKeys),Now=nowUtc,Actor=actor
        },cancellationToken).ConfigureAwait(false)).Single();
        return new(row.Result,row.MenuId);
    }

    private async Task<MenuImportMutationOutcome> MutateAsync<T>(string sql, T parameters, CancellationToken cancellationToken)
    {
        var result = (await dataAccess.ExecuteSqlQueryAsync<ResultRow, T>(sql, parameters, cancellationToken).ConfigureAwait(false)).Single();
        var parameterType = parameters!.GetType();
        var venueId = (Guid)parameterType.GetProperty("VenueId")!.GetValue(parameters)!;
        var sessionId = (Guid)parameterType.GetProperty("SessionId")!.GetValue(parameters)!;
        var now = (DateTime)parameterType.GetProperty("Now")!.GetValue(parameters)!;
        if (result.Result is MenuImportMutationOutcome.NotFound or MenuImportMutationOutcome.Expired or MenuImportMutationOutcome.Invalid)
            return new(result.Result, null);
        return new(result.Result, await GetAsync(venueId, sessionId, now, cancellationToken).ConfigureAwait(false));
    }

    private static object Parameters(MenuImportAggregate aggregate, byte[]? expectedRevision = null) => new
    {
        SessionId = aggregate.Session.Id, aggregate.Session.VenueId, aggregate.Session.RawPaste, aggregate.Session.ParseRevision,
        aggregate.Session.Status, aggregate.Session.LineCount, aggregate.Session.ItemCount, aggregate.Session.ExpiresUtc,
        aggregate.Session.CreatedUtc, aggregate.Session.UpdatedUtc, aggregate.Session.UpdatedBy, ExpectedRevision = expectedRevision,
        aggregate.Session.SuggestedMenuName, aggregate.Session.SuggestedMenuDescription,
        Now = aggregate.Session.UpdatedUtc,
        LinesJson = JsonSerializer.Serialize(aggregate.Lines),
        QuestionsJson = JsonSerializer.Serialize(aggregate.Questions.Select(q => new QuestionPayload(q.QuestionKey, q.Fingerprint, q.Kind, q.DisplayOrder, q.Required, q.ParseRevision))),
        QuestionLinesJson = JsonSerializer.Serialize(aggregate.Questions.SelectMany(q => q.LineNumbers.Select(line => new QuestionLinePayload(q.QuestionKey, line)))),
        CandidatesJson = JsonSerializer.Serialize(aggregate.Questions.SelectMany(q => q.Candidates.Select(c => new CandidatePayload(
            q.QuestionKey, c.ItemId, c.DisplayName, c.DisplayPrice, c.MatchRule, c.IsSafe,
            c.OnMenus is null ? null : JsonSerializer.Serialize(c.OnMenus), c.ItemCreatedUtc)))),
        AnswersJson = JsonSerializer.Serialize(aggregate.Questions.Where(q => q.Answer is not null)
            .Select(q => new AnswerPayload(q.QuestionKey, q.Fingerprint, q.Answer!.Choice, q.Answer.SelectedItemId, q.ParseRevision)))
    };

    private static MenuImportAggregate Hydrate(AggregateRow row)
    {
        var lines = JsonSerializer.Deserialize<List<MenuImportSourceLine>>(row.LinesJson ?? "[]", JsonOptions) ?? [];
        var questions = JsonSerializer.Deserialize<List<QuestionRead>>(row.QuestionsJson ?? "[]", JsonOptions) ?? [];
        var session = new MenuImportSession(row.Id, row.VenueId, row.RawPaste, row.ParseRevision, row.Status, row.LineCount,
            row.ItemCount, row.ExpiresUtc, row.CreatedUtc, row.UpdatedUtc, row.UpdatedBy, row.Revision,
            row.Destination, row.ProposedMenuName, row.CompletedMenuId, row.CompletedUtc,
            row.TargetMenuId,row.TargetUpdatedUtc,row.CompletedSnapshotId,row.TargetMenuName,row.TargetHadPublishedVersion,
            row.TargetWorkingItemCount,row.TargetPublishedItemCount,row.TargetAddedCount,row.TargetRemovedCount,row.TargetChangedCount,row.CompletedSnapshotRestoredUtc,
            row.ProposedMenuDescription, row.SuggestedMenuName, row.SuggestedMenuDescription);
        return new(session, lines, questions.Select(q => new MenuImportReviewQuestion(session.Id, session.VenueId,
            q.QuestionKey, q.Fingerprint, q.Kind, q.DisplayOrder, q.Required, q.ParseRevision, q.LineNumbers ?? [], q.Candidates ?? [], q.Answer)).ToArray());
    }

    private static void ValidateAggregate(MenuImportAggregate aggregate)
    {
        RequireId(aggregate.Session.Id, nameof(aggregate.Session.Id));
        RequireId(aggregate.Session.VenueId, nameof(aggregate.Session.VenueId));
        if (aggregate.Lines.Any(x => x.SessionId != aggregate.Session.Id || x.VenueId != aggregate.Session.VenueId || x.ParseRevision != aggregate.Session.ParseRevision) ||
            aggregate.Questions.Any(x => x.SessionId != aggregate.Session.Id || x.VenueId != aggregate.Session.VenueId || x.ParseRevision != aggregate.Session.ParseRevision))
            throw new ArgumentException("All import rows must belong to the session venue and parse revision.", nameof(aggregate));
    }

    private static Guid RequireId(Guid value, string name) => value == Guid.Empty ? throw new ArgumentException("A non-empty id is required.", name) : value;
    private static byte[] RequireRevision(byte[] value) => value is { Length: 8 } ? value : throw new ArgumentException("An 8-byte revision is required.", nameof(value));
    private static string RequireText(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A value is required.", name) : value;
    private static string RequireChoice(string value) => value is MenuImportChoices.SameItem or MenuImportChoices.NewItem or MenuImportChoices.Section or MenuImportChoices.Fallback or MenuImportChoices.LeaveOut ? value : throw new ArgumentOutOfRangeException(nameof(value));
    private static string RequireAnswerShape(string choice, Guid? selectedItemId)
    {
        RequireChoice(choice);
        if ((choice == MenuImportChoices.SameItem) != selectedItemId.HasValue)
            throw new ArgumentException("Same-item answers require a selected item; other answers cannot select one.", nameof(selectedItemId));
        return choice;
    }
    private static string RequireFingerprint(string value) => value is { Length: 64 } ? value : throw new ArgumentException("A 64-character fingerprint is required.", nameof(value));
    private static string RequireMenuName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        return normalized.Length <= 200 ? normalized : throw new ArgumentException("Menu name cannot exceed 200 characters.", nameof(value));
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private sealed record QuestionPayload(string QuestionKey, string Fingerprint, string Kind, int DisplayOrder, bool Required, long ParseRevision);
    private sealed record QuestionLinePayload(string QuestionKey, int LineNumber);

    private sealed record AnswerPayload(string QuestionKey, string Fingerprint, string Choice, Guid? SelectedItemId, long ParseRevision);
    private sealed record CandidatePayload(string QuestionKey, Guid ItemId, string DisplayName, string? DisplayPrice, string MatchRule, bool IsSafe,
        string? OnMenusJson, DateTime? ItemCreatedUtc);
    private sealed record QuestionRead(string QuestionKey, string Fingerprint, string Kind, int DisplayOrder, bool Required, long ParseRevision,
        IReadOnlyCollection<int>? LineNumbers, IReadOnlyCollection<MenuImportCandidate>? Candidates, MenuImportAnswer? Answer);
    private sealed record ResultRow(string Result);
    private sealed record CreateResultRow(string Result, Guid? MenuId);
    private sealed record ReplaceDestinationRow(string Result,Guid? MenuId,string? MenuName,DateTime? TargetUpdatedUtc,
        bool HasPublishedVersion,int WorkingItemCount,int PublishedItemCount,int Added,int Removed,int Changed);
    private sealed record CountRow(int Count);
    private sealed class AggregateRow
    {
        public Guid Id { get; init; }
        public Guid VenueId { get; init; }
        public string RawPaste { get; init; } = "";
        public long ParseRevision { get; init; }
        public string Status { get; init; } = "";
        public int LineCount { get; init; }
        public int ItemCount { get; init; }
        public DateTime ExpiresUtc { get; init; }
        public DateTime CreatedUtc { get; init; }
        public DateTime UpdatedUtc { get; init; }
        public string? UpdatedBy { get; init; }
        public string? Destination { get; init; }
        public string? ProposedMenuName { get; init; }

        public string? ProposedMenuDescription { get; init; }

        public string? SuggestedMenuName { get; init; }

        public string? SuggestedMenuDescription { get; init; }
        public Guid? CompletedMenuId { get; init; }
        public DateTime? CompletedUtc { get; init; }
        public Guid? TargetMenuId { get; init; }
        public DateTime? TargetUpdatedUtc { get; init; }
        public Guid? CompletedSnapshotId { get; init; }
        public string? TargetMenuName { get; init; }
        public bool? TargetHadPublishedVersion { get; init; }
        public int? TargetWorkingItemCount { get; init; }
        public int? TargetPublishedItemCount { get; init; }
        public int? TargetAddedCount { get; init; }
        public int? TargetRemovedCount { get; init; }
        public int? TargetChangedCount { get; init; }
        public DateTime? CompletedSnapshotRestoredUtc { get; init; }
        public byte[] Revision { get; init; } = [];
        public string? LinesJson { get; init; } = "[]";
        public string? QuestionsJson { get; init; } = "[]";
    }

    private const string InsertDerivedSql = """
INSERT dbo.MenuImportSourceLines (SessionId, VenueId, LineNumber, LineSubIndex, RawText, Disposition, ParsedName, ParsedDescription, ParsedPrice, ParserReason, ParseRevision, SuggestedVerdict, SuggestedReason)
SELECT @SessionId, @VenueId, LineNumber, LineSubIndex, RawText, Disposition, ParsedName, ParsedDescription, ParsedPrice, ParserReason, ParseRevision, SuggestedVerdict, SuggestedReason
FROM OPENJSON(@LinesJson) WITH (LineNumber int, LineSubIndex int, RawText nvarchar(max), Disposition nvarchar(24), ParsedName nvarchar(200), ParsedDescription nvarchar(1000), ParsedPrice nvarchar(12), ParserReason nvarchar(80), ParseRevision bigint, SuggestedVerdict nvarchar(24), SuggestedReason nvarchar(300));
INSERT dbo.MenuImportReviewQuestions (SessionId, VenueId, QuestionKey, Fingerprint, Kind, DisplayOrder, Required, ParseRevision)
SELECT @SessionId, @VenueId, QuestionKey, Fingerprint, Kind, DisplayOrder, Required, ParseRevision
FROM OPENJSON(@QuestionsJson) WITH (QuestionKey nvarchar(80), Fingerprint char(64), Kind nvarchar(32), DisplayOrder int, Required bit, ParseRevision bigint);
INSERT dbo.MenuImportQuestionLines (SessionId, VenueId, QuestionKey, LineNumber, LineSubIndex)
SELECT @SessionId, @VenueId, QuestionKey, LineNumber, 0 FROM OPENJSON(@QuestionLinesJson) WITH (QuestionKey nvarchar(80), LineNumber int);
-- OnMenusJson and ItemCreatedUtc are A21: where a question offers more than one candidate, they
-- are what tells two identical-looking rows apart. Null on a single candidate, which has nothing
-- to be distinguished from.
INSERT dbo.MenuImportCandidates (SessionId, VenueId, QuestionKey, ItemId, DisplayName, DisplayPrice, MatchRule, IsSafe, OnMenusJson, ItemCreatedUtc)
SELECT @SessionId, @VenueId, QuestionKey, ItemId, DisplayName, DisplayPrice, MatchRule, IsSafe, OnMenusJson, ItemCreatedUtc
FROM OPENJSON(@CandidatesJson) WITH (QuestionKey nvarchar(80), ItemId uniqueidentifier, DisplayName nvarchar(200), DisplayPrice nvarchar(12), MatchRule nvarchar(32), IsSafe bit, OnMenusJson nvarchar(1000), ItemCreatedUtc datetime2(7));
-- Answers the parser was able to give itself. A18 forbids pre-answering unless a rule can name
-- why; these carry `exact_normalized` as their match rule and are only ever written where the name
-- matched after case, spacing and punctuation AND the price is the same. The question is still
-- recorded, so the operator can find it under "Review all N pasted lines" and change it.
INSERT dbo.MenuImportAnswers (SessionId, VenueId, QuestionKey, Fingerprint, Choice, SelectedItemId, ParseRevision, AnsweredUtc, AnsweredBy)
SELECT @SessionId, @VenueId, QuestionKey, Fingerprint, Choice, SelectedItemId, ParseRevision, @UpdatedUtc, NULL
FROM OPENJSON(@AnswersJson) WITH (QuestionKey nvarchar(80), Fingerprint char(64), Choice nvarchar(24), SelectedItemId uniqueidentifier, ParseRevision bigint);
""";

    private static readonly string CreateSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION;
INSERT dbo.MenuImportSessions (Id, VenueId, RawPaste, ParseRevision, Status, LineCount, ItemCount, ExpiresUtc, CreatedUtc, UpdatedUtc, UpdatedBy, SuggestedMenuName, SuggestedMenuDescription)
VALUES (@SessionId, @VenueId, @RawPaste, @ParseRevision, @Status, @LineCount, @ItemCount, @ExpiresUtc, @CreatedUtc, @UpdatedUtc, @UpdatedBy, @SuggestedMenuName, @SuggestedMenuDescription);
""" + InsertDerivedSql + "COMMIT; SELECT N'updated' Result;";

    private const string ReadSql = """
SELECT s.Id, s.VenueId, s.RawPaste, s.ParseRevision, s.Status, s.LineCount, s.ItemCount, s.ExpiresUtc, s.CreatedUtc, s.UpdatedUtc, s.UpdatedBy, s.Revision,
 s.Destination, s.ProposedMenuName, s.ProposedMenuDescription, s.SuggestedMenuName, s.SuggestedMenuDescription, s.CompletedMenuId, s.CompletedUtc,s.TargetMenuId,s.TargetUpdatedUtc,s.CompletedSnapshotId,
 s.TargetMenuName,s.TargetHadPublishedVersion,s.TargetWorkingItemCount,s.TargetPublishedItemCount,s.TargetAddedCount,s.TargetRemovedCount,s.TargetChangedCount,
 (SELECT RestoredUtc FROM dbo.MenuImportReplacementSnapshots rs WHERE rs.Id=s.CompletedSnapshotId) CompletedSnapshotRestoredUtc,
 (SELECT l.SessionId, l.VenueId, l.LineNumber, l.LineSubIndex, l.RawText, l.Disposition, l.ParsedName, l.ParsedDescription, l.ParsedPrice, l.ParserReason, l.ParseRevision, l.SuggestedVerdict, l.SuggestedReason FROM dbo.MenuImportSourceLines l WHERE l.SessionId=s.Id ORDER BY l.LineNumber, l.LineSubIndex FOR JSON PATH) LinesJson,
 (SELECT q.QuestionKey, q.Fingerprint, q.Kind, q.DisplayOrder, q.Required, q.ParseRevision,
   JSON_QUERY(COALESCE((SELECT N'['+STRING_AGG(CONVERT(nvarchar(max), ql.LineNumber),N',') WITHIN GROUP (ORDER BY ql.LineNumber)+N']' FROM dbo.MenuImportQuestionLines ql WHERE ql.SessionId=q.SessionId AND ql.QuestionKey=q.QuestionKey),N'[]')) LineNumbers,
   JSON_QUERY((SELECT c.ItemId, c.DisplayName, c.DisplayPrice, c.MatchRule, c.IsSafe, JSON_QUERY(c.OnMenusJson) OnMenus, c.ItemCreatedUtc FROM dbo.MenuImportCandidates c WHERE c.SessionId=q.SessionId AND c.QuestionKey=q.QuestionKey ORDER BY c.DisplayName, c.ItemId FOR JSON PATH)) Candidates,
   JSON_QUERY((SELECT a.Fingerprint, a.Choice, a.SelectedItemId, a.ParseRevision, a.AnsweredUtc, a.AnsweredBy FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)) Answer
  FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=s.Id ORDER BY q.DisplayOrder, q.QuestionKey FOR JSON PATH) QuestionsJson
FROM dbo.MenuImportSessions s WHERE s.Id=@SessionId AND s.VenueId=@VenueId AND s.ExpiresUtc>@Now;
""";

    private const string SetCreateDestinationSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION; DECLARE @Result nvarchar(20)=N'updated';
DECLARE @CurrentRevision varbinary(8), @CurrentExpiry datetime2, @Status nvarchar(20), @Completed uniqueidentifier;
SELECT @CurrentRevision=Revision,@CurrentExpiry=ExpiresUtc,@Status=Status,@Completed=CompletedMenuId
FROM dbo.MenuImportSessions WITH (UPDLOCK,HOLDLOCK) WHERE Id=@SessionId AND VenueId=@VenueId;
IF @CurrentRevision IS NULL SET @Result=N'not_found';
ELSE IF @CurrentExpiry<=@Now SET @Result=N'expired';
ELSE IF @CurrentRevision<>@ExpectedRevision SET @Result=N'conflict';
ELSE IF @Status<>N'resolved' OR @Completed IS NOT NULL SET @Result=N'invalid';
IF @Result=N'updated' UPDATE dbo.MenuImportSessions
 SET Destination=N'create',ProposedMenuName=@MenuName,TargetMenuId=NULL,TargetUpdatedUtc=NULL,TargetWorkingFingerprint=NULL,TargetMenuName=NULL,
 TargetHadPublishedVersion=NULL,TargetWorkingItemCount=NULL,TargetPublishedItemCount=NULL,TargetAddedCount=NULL,TargetRemovedCount=NULL,TargetChangedCount=NULL,
 UpdatedUtc=@Now,UpdatedBy=@Actor WHERE Id=@SessionId;
COMMIT; SELECT @Result Result;
""";

    private const string ConfirmCreateSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION;
DECLARE @Result nvarchar(24)=N'created', @ExistingMenu uniqueidentifier, @Name nvarchar(200), @Expiry datetime2,
 @Revision varbinary(8), @Status nvarchar(20), @Active int, @OrganizationId uniqueidentifier,
 @MenuLimit int=@DefaultMenuLimit, @ItemLimit int=@DefaultItemLimit;
SELECT @ExistingMenu=CompletedMenuId,@Name=ProposedMenuName,@Expiry=ExpiresUtc,@Revision=Revision,@Status=Status
FROM dbo.MenuImportSessions WITH (UPDLOCK,HOLDLOCK) WHERE Id=@SessionId AND VenueId=@VenueId;
IF @Revision IS NULL SET @Result=N'not_found';
ELSE IF @ExistingMenu IS NOT NULL BEGIN SET @Result=N'already_completed'; SET @MenuId=@ExistingMenu; END
ELSE IF @Expiry<=@Now SET @Result=N'expired';
ELSE IF @Revision<>@ExpectedRevision SET @Result=N'conflict';
ELSE IF @Status<>N'resolved' OR @Name IS NULL SET @Result=N'invalid';
ELSE IF EXISTS(SELECT 1 FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=@SessionId AND q.Required=1
 AND NOT EXISTS(SELECT 1 FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey AND a.Fingerprint=q.Fingerprint)) SET @Result=N'invalid';
IF @Result=N'created' BEGIN
 SELECT @OrganizationId=OrganizationId FROM dbo.Venues WITH (UPDLOCK,HOLDLOCK) WHERE Id=@VenueId;
 IF NOT EXISTS(
  SELECT 1 FROM OPENJSON(@SystemRolesJson) roles
  JOIN dbo.AuthorityRolePermissions rp WITH (UPDLOCK,HOLDLOCK) ON rp.RoleKey=roles.[value] AND rp.PermissionId=N'content.menu.import'
  UNION ALL
  SELECT 1 FROM dbo.ScopedRoleAssignments assignment WITH (UPDLOCK,HOLDLOCK)
  JOIN dbo.AuthorityRolePermissions rp WITH (UPDLOCK,HOLDLOCK) ON rp.RoleKey=assignment.RoleKey AND rp.PermissionId=N'content.menu.import'
  WHERE assignment.ActorUserId=@ActorUserId AND assignment.RevokedUtc IS NULL AND assignment.StartsUtc<=@Now
   AND (assignment.ExpiresUtc IS NULL OR assignment.ExpiresUtc>@Now)
   AND ((assignment.ScopeType=4 AND assignment.ScopeId=@VenueId) OR (assignment.ScopeType=2 AND assignment.ScopeId=@OrganizationId))
 ) SET @Result=N'permission_denied';
END
IF @Result=N'created' BEGIN
 SELECT TOP (1) @MenuLimit=LimitValue FROM dbo.CapabilityAllowances WITH (UPDLOCK,HOLDLOCK)
 WHERE CapabilityId=N'content.menu.count' AND (VenueId=@VenueId OR (VenueId IS NULL AND OrganizationId=@OrganizationId))
  AND StartsUtc<=@Now AND (EndsUtc IS NULL OR EndsUtc>@Now)
 ORDER BY CASE WHEN VenueId=@VenueId THEN 0 ELSE 1 END,StartsUtc DESC;
 SELECT TOP (1) @ItemLimit=LimitValue FROM dbo.CapabilityAllowances WITH (UPDLOCK,HOLDLOCK)
 WHERE CapabilityId=N'content.menu.items' AND (VenueId=@VenueId OR (VenueId IS NULL AND OrganizationId=@OrganizationId))
  AND StartsUtc<=@Now AND (EndsUtc IS NULL OR EndsUtc>@Now)
 ORDER BY CASE WHEN VenueId=@VenueId THEN 0 ELSE 1 END,StartsUtc DESC;
 SELECT @Active=COUNT(*) FROM dbo.Menus WITH (UPDLOCK,HOLDLOCK) WHERE VenueId=@VenueId AND IsPutAway=0;
 IF @Active+1>@MenuLimit SET @Result=N'menu_limit';
 ELSE IF EXISTS(SELECT 1 FROM dbo.Menus WHERE VenueId=@VenueId AND UPPER(LTRIM(RTRIM(Name)))=UPPER(@Name)) SET @Result=N'name_conflict';
END
IF @Result=N'created' BEGIN
 CREATE TABLE #Sections(LineNumber bigint PRIMARY KEY, SectionId uniqueidentifier NOT NULL, Name nvarchar(200) NOT NULL, SortOrder int NOT NULL);
 INSERT #Sections SELECT CONVERT(bigint,LineNumber)*1000+LineSubIndex,NEWID(),ParsedName,ROW_NUMBER() OVER(ORDER BY LineNumber,LineSubIndex)-1
 FROM dbo.MenuImportSourceLines WHERE SessionId=@SessionId AND Disposition=N'section';
 CREATE TABLE #Rows(LineNumber bigint PRIMARY KEY,ItemId uniqueidentifier NOT NULL,Existing bit NOT NULL,Fallback bit NOT NULL,Name nvarchar(200) NULL,Description nvarchar(1000) NULL,Price nvarchar(12) NULL,SectionId uniqueidentifier NULL,SortOrder int NULL);
 INSERT #Rows(LineNumber,ItemId,Existing,Fallback,Name,Description,Price)
 SELECT CONVERT(bigint,l.LineNumber)*1000+l.LineSubIndex,COALESCE(a.SelectedItemId,NEWID()),CASE WHEN a.SelectedItemId IS NULL THEN 0 ELSE 1 END,
  CASE WHEN l.Disposition=N'unresolved' THEN 1 ELSE 0 END,
  CASE WHEN l.Disposition=N'item' THEN l.ParsedName ELSE NULLIF(LTRIM(RTRIM(l.RawText)),N'') END,l.ParsedDescription,l.ParsedPrice
 FROM dbo.MenuImportSourceLines l LEFT JOIN dbo.MenuImportQuestionLines ql ON ql.SessionId=l.SessionId AND ql.LineNumber=l.LineNumber AND l.LineSubIndex=0
 LEFT JOIN dbo.MenuImportAnswers a ON a.SessionId=ql.SessionId AND a.QuestionKey=ql.QuestionKey
 WHERE l.SessionId=@SessionId AND (l.Disposition=N'item' OR (l.Disposition=N'unresolved' AND a.Choice=N'fallback'));
 /*
  * The library holds a dish once (migration 082), so a confirm LINKS to what is already there
  * rather than minting a second copy - which the unique index would now refuse outright.
  *
  * Two steps, and both are needed:
  *   1. A row the operator did not answer, whose name canonically matches a library item, becomes
  *      that item. Setting Existing=1 also makes the pasted price the PLACEMENT's price below,
  *      which is what "same item, printed here at this price" means (A19) and exactly what an
  *      operator-answered match already does.
  *   2. Rows left over that share a name share ONE new id, so one paste naming a dish twice
  *      creates it once. The parser stops asking twice (#938); this stops writing twice.
  */
 UPDATE r SET ItemId=existing.Id, Existing=1
 FROM #Rows r CROSS APPLY (
   SELECT TOP 1 i.Id FROM dbo.Items i
   WHERE i.VenueId=@VenueId AND i.IsActive=1 AND i.CanonicalName=dbo.CanonicalItemName(r.Name)
   ORDER BY i.CreatedUtc,i.Id) existing
 WHERE r.Existing=0 AND r.Name IS NOT NULL;

 WITH shared AS (
   SELECT LineNumber, FIRST_VALUE(ItemId) OVER (PARTITION BY dbo.CanonicalItemName(Name) ORDER BY LineNumber) AS Id
   FROM #Rows WHERE Existing=0)
 UPDATE r SET ItemId=shared.Id FROM #Rows r JOIN shared ON shared.LineNumber=r.LineNumber WHERE r.Existing=0;
 IF (SELECT COUNT(*) FROM #Rows)>@ItemLimit SET @Result=N'item_limit';
 -- Two rows sharing an item is now the intended shape, not a fault: the same dish printed twice is
 -- one library item placed twice. Name validity still refuses.
 ELSE IF EXISTS(SELECT 1 FROM #Rows WHERE Name IS NULL OR LEN(Name)>200) SET @Result=N'invalid_content';
 ELSE BEGIN
  IF EXISTS(SELECT 1 FROM #Rows r WHERE r.Fallback=1 OR NOT EXISTS(SELECT 1 FROM #Sections s WHERE s.LineNumber<r.LineNumber)) INSERT #Sections VALUES(0,NEWID(),N'Imported items',-1);
  UPDATE r SET SectionId=CASE WHEN r.Fallback=1 THEN (SELECT SectionId FROM #Sections WHERE LineNumber=0) ELSE COALESCE((SELECT TOP 1 s.SectionId FROM #Sections s WHERE s.LineNumber<r.LineNumber ORDER BY s.LineNumber DESC),(SELECT SectionId FROM #Sections WHERE LineNumber=0)) END FROM #Rows r;
  WITH ranked AS (SELECT LineNumber,ROW_NUMBER() OVER(PARTITION BY SectionId ORDER BY LineNumber)-1 SortOrder FROM #Rows) UPDATE r SET SortOrder=x.SortOrder FROM #Rows r JOIN ranked x ON x.LineNumber=r.LineNumber;
  INSERT dbo.Menus(Id,VenueId,Name,IsActive,CreatedUtc,UpdatedUtc) VALUES(@MenuId,@VenueId,@Name,1,@Now,@Now);
  INSERT dbo.MenuPages(Id,VenueId,MenuId,Name,SortOrder,CreatedUtc,UpdatedUtc) VALUES(@PageId,@VenueId,@MenuId,N'Page 1',0,@Now,@Now);
  INSERT dbo.MenuSections(Id,VenueId,MenuId,PageId,Name,SortOrder,CreatedUtc,UpdatedUtc) SELECT SectionId,@VenueId,@MenuId,@PageId,Name,ROW_NUMBER() OVER(ORDER BY SortOrder,LineNumber)-1,@Now,@Now FROM #Sections;
  -- One row per NEW item: rows sharing an id above are one dish, and the first line's description
  -- and price become the library default. Each placement keeps its own price below.
  INSERT dbo.Items(Id,VenueId,Name,Description,Price,Source,IsActive,CreatedUtc,UpdatedUtc)
   SELECT ItemId,@VenueId,Name,Description,Price,N'import',1,@Now,@Now
   FROM (SELECT ItemId,Name,Description,Price,ROW_NUMBER() OVER(PARTITION BY ItemId ORDER BY LineNumber) AS Pick
         FROM #Rows WHERE Existing=0) first WHERE first.Pick=1;
  /*
   * One placement per item on the page, first line wins. "An item appears at most once on a page"
   * is a model invariant (M3-A, migration 062) reaffirmed by the owner on 2026-08-28, and an
   * import writes one page. A menu printing the same dish twice keeps the first placement.
   */
  CREATE TABLE #Placed(LineNumber int PRIMARY KEY,PlacementId uniqueidentifier NOT NULL);
  INSERT #Placed SELECT LineNumber,NEWID() FROM (
    SELECT LineNumber,ROW_NUMBER() OVER(PARTITION BY ItemId ORDER BY LineNumber) AS Pick FROM #Rows) once WHERE once.Pick=1;
  INSERT dbo.Placements(Id,VenueId,MenuId,MenuSectionId,PageId,ItemId,SortOrder,CreatedUtc,UpdatedUtc,ImportedPriceOverride)
   SELECT p.PlacementId,@VenueId,@MenuId,r.SectionId,@PageId,r.ItemId,r.SortOrder,@Now,@Now,CASE WHEN r.Existing=1 THEN r.Price END FROM #Rows r JOIN #Placed p ON p.LineNumber=r.LineNumber;
  INSERT dbo.MenuImportCreatedLines(SessionId,VenueId,LineNumber,MenuId,MenuSectionId,PlacementId) SELECT @SessionId,@VenueId,r.LineNumber,@MenuId,r.SectionId,p.PlacementId FROM #Rows r JOIN #Placed p ON p.LineNumber=r.LineNumber;
  UPDATE dbo.MenuImportSessions SET ItemCount=(SELECT COUNT(*) FROM #Rows),CompletedMenuId=@MenuId,CompletedUtc=@Now,UpdatedUtc=@Now,UpdatedBy=@Actor WHERE Id=@SessionId;
 END
END
IF @Result<>N'created' AND @Result<>N'already_completed' SET @MenuId=NULL;
COMMIT; SELECT @Result Result,@MenuId MenuId;
""";

    private const string SetReplaceDestinationSql = """
SET XACT_ABORT ON;BEGIN TRANSACTION;DECLARE @Result nvarchar(24)=N'updated',@Revision varbinary(8),@Expiry datetime2,@Status nvarchar(20),@Completed uniqueidentifier,@MenuUpdated datetime2,@MenuName nvarchar(200),@Published nvarchar(max);
SELECT @Revision=Revision,@Expiry=ExpiresUtc,@Status=Status,@Completed=CompletedMenuId FROM dbo.MenuImportSessions WITH(UPDLOCK,HOLDLOCK) WHERE Id=@SessionId AND VenueId=@VenueId;
IF @Revision IS NULL SET @Result=N'not_found'; ELSE IF @Completed IS NOT NULL SET @Result=N'already_completed'; ELSE IF @Expiry<=@Now SET @Result=N'expired'; ELSE IF @Revision<>@ExpectedRevision SET @Result=N'conflict'; ELSE IF @Status<>N'resolved' SET @Result=N'invalid';
IF @Result=N'updated' BEGIN SELECT @MenuUpdated=UpdatedUtc,@MenuName=Name FROM dbo.Menus WITH(UPDLOCK,HOLDLOCK) WHERE Id=@MenuId AND VenueId=@VenueId AND IsPutAway=0;IF @MenuUpdated IS NULL SET @Result=N'target_missing';END
IF @Result=N'updated' BEGIN
 SELECT TOP(1) @Published=Snapshot FROM dbo.MenuPublishEvents WHERE VenueId=@VenueId AND MenuId=@MenuId ORDER BY Version DESC;
 -- Counts are stored with the selected target so refresh resumes the same explanation.
END
DECLARE @Working int=0,@PublishedCount int=0,@Added int=0,@Removed int=0,@Changed int=0,@WorkingSnapshot nvarchar(max),@WorkingFingerprint char(64);
IF @Result=N'updated' BEGIN
 SELECT @WorkingSnapshot=(SELECT m.Id menuId,m.Name name,m.Theme theme,m.DwellSeconds dwellSeconds,m.LoopWarningSeconds loopWarningSeconds,JSON_QUERY((SELECT CAST(a.ScreenId AS nvarchar(36))screenId,a.PageId pageId FROM dbo.MenuScreenAssignments a WITH(UPDLOCK,HOLDLOCK) WHERE a.MenuId=m.Id AND a.VenueId=@VenueId ORDER BY a.ScreenId FOR JSON PATH))screens,JSON_QUERY((SELECT p.Id pageId,p.Name name,p.SortOrder sortOrder FROM dbo.MenuPages p WITH(UPDLOCK,HOLDLOCK) WHERE p.MenuId=m.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))pages,JSON_QUERY((SELECT s.Id sectionId,s.PageId pageId,s.Name name,s.SortOrder sortOrder,JSON_QUERY((SELECT p.ItemId itemId,i.Name name,i.Description description,COALESCE(p.ImportedPriceOverride,i.Price)price,p.ImportedPriceOverride importedPriceOverride,p.SortOrder sortOrder FROM dbo.Placements p WITH(UPDLOCK,HOLDLOCK) JOIN dbo.Items i WITH(UPDLOCK,HOLDLOCK) ON i.Id=p.ItemId AND i.VenueId=p.VenueId WHERE p.MenuSectionId=s.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))items FROM dbo.MenuSections s WITH(UPDLOCK,HOLDLOCK) WHERE s.MenuId=m.Id AND s.VenueId=@VenueId ORDER BY s.SortOrder,s.Id FOR JSON PATH))sections FROM dbo.Menus m WHERE m.Id=@MenuId AND m.VenueId=@VenueId FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
 SET @WorkingFingerprint=CONVERT(char(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),@WorkingSnapshot)),2);
 SELECT @Working=COUNT(*) FROM dbo.Placements WHERE VenueId=@VenueId AND MenuId=@MenuId;
 SELECT @PublishedCount=COUNT(*) FROM OPENJSON(@Published,'$.sections') s CROSS APPLY OPENJSON(s.value,'$.items');
 ;WITH w AS(SELECT p.ItemId,COALESCE(p.ImportedPriceOverride,i.Price) Price FROM dbo.Placements p JOIN dbo.Items i ON i.Id=p.ItemId WHERE p.VenueId=@VenueId AND p.MenuId=@MenuId),
 p AS(SELECT itemId,price FROM OPENJSON(@Published,'$.sections') s CROSS APPLY OPENJSON(s.value,'$.items') WITH(itemId uniqueidentifier '$.itemId',price nvarchar(40) '$.price'))
 SELECT @Added=SUM(CASE WHEN p.itemId IS NULL THEN 1 ELSE 0 END),@Changed=SUM(CASE WHEN p.itemId IS NOT NULL AND ISNULL(w.Price,N'')<>ISNULL(p.price,N'') THEN 1 ELSE 0 END) FROM w LEFT JOIN p ON p.itemId=w.ItemId;
 ;WITH w AS(SELECT ItemId FROM dbo.Placements WHERE VenueId=@VenueId AND MenuId=@MenuId),p AS(SELECT itemId FROM OPENJSON(@Published,'$.sections') s CROSS APPLY OPENJSON(s.value,'$.items') WITH(itemId uniqueidentifier '$.itemId')) SELECT @Removed=COUNT(*) FROM p LEFT JOIN w ON w.ItemId=p.itemId WHERE w.ItemId IS NULL;
 UPDATE dbo.MenuImportSessions SET Destination=N'replace',ProposedMenuName=NULL,TargetMenuId=@MenuId,TargetUpdatedUtc=@MenuUpdated,TargetWorkingFingerprint=@WorkingFingerprint,TargetMenuName=@MenuName,TargetHadPublishedVersion=IIF(@Published IS NULL,0,1),TargetWorkingItemCount=@Working,TargetPublishedItemCount=@PublishedCount,TargetAddedCount=ISNULL(@Added,0),TargetRemovedCount=@Removed,TargetChangedCount=ISNULL(@Changed,0),UpdatedUtc=@Now,UpdatedBy=@Actor WHERE Id=@SessionId;
END
COMMIT;SELECT @Result Result,CASE WHEN @Result=N'updated' THEN @MenuId END MenuId,@MenuName MenuName,@MenuUpdated TargetUpdatedUtc,CONVERT(bit,CASE WHEN @Published IS NULL THEN 0 ELSE 1 END) HasPublishedVersion,@Working WorkingItemCount,@PublishedCount PublishedItemCount,ISNULL(@Added,0) Added,@Removed Removed,ISNULL(@Changed,0) Changed;
""";

    private const string ConfirmReplaceSql = """
SET XACT_ABORT ON;BEGIN TRANSACTION;
DECLARE @Result nvarchar(24)=N'created',@MenuId uniqueidentifier,@TargetRevision datetime2,@TargetFingerprint char(64),@CurrentTargetRevision datetime2,@CurrentFingerprint char(64),@PostFingerprint char(64),@Expiry datetime2,@Revision varbinary(8),@Status nvarchar(20),@Existing uniqueidentifier,@OrganizationId uniqueidentifier,@ItemLimit int=@DefaultItemLimit,@RetentionDays int=30,@Snapshot nvarchar(max),@PostSnapshot nvarchar(max),@PageId uniqueidentifier;
SELECT @Existing=CompletedMenuId,@MenuId=TargetMenuId,@TargetRevision=TargetUpdatedUtc,@TargetFingerprint=TargetWorkingFingerprint,@Expiry=ExpiresUtc,@Revision=Revision,@Status=Status FROM dbo.MenuImportSessions WITH(UPDLOCK,HOLDLOCK) WHERE Id=@SessionId AND VenueId=@VenueId;
IF @Revision IS NULL SET @Result=N'not_found';ELSE IF @Existing IS NOT NULL BEGIN SET @Result=N'already_completed';SET @MenuId=@Existing;END ELSE IF @Expiry<=@Now SET @Result=N'expired';ELSE IF @Revision<>@ExpectedRevision SET @Result=N'conflict';ELSE IF @Status<>N'resolved' OR @MenuId IS NULL SET @Result=N'invalid';
IF @Result=N'created' BEGIN SELECT @OrganizationId=OrganizationId FROM dbo.Venues WITH(UPDLOCK,HOLDLOCK) WHERE Id=@VenueId;
 IF NOT EXISTS(SELECT 1 FROM OPENJSON(@SystemRolesJson) r JOIN dbo.AuthorityRolePermissions rp WITH(UPDLOCK,HOLDLOCK) ON rp.RoleKey=r.value AND rp.PermissionId=N'content.menu.import' UNION ALL SELECT 1 FROM dbo.ScopedRoleAssignments a WITH(UPDLOCK,HOLDLOCK) JOIN dbo.AuthorityRolePermissions rp WITH(UPDLOCK,HOLDLOCK) ON rp.RoleKey=a.RoleKey AND rp.PermissionId=N'content.menu.import' WHERE a.ActorUserId=@ActorUserId AND a.RevokedUtc IS NULL AND a.StartsUtc<=@Now AND(a.ExpiresUtc IS NULL OR a.ExpiresUtc>@Now)AND((a.ScopeType=4 AND a.ScopeId=@VenueId)OR(a.ScopeType=2 AND a.ScopeId=@OrganizationId))) SET @Result=N'permission_denied';
END
IF @Result=N'created' BEGIN SELECT @CurrentTargetRevision=UpdatedUtc,@PageId=(SELECT TOP(1) Id FROM dbo.MenuPages WITH(UPDLOCK,HOLDLOCK) WHERE VenueId=@VenueId AND MenuId=@MenuId ORDER BY SortOrder,Id) FROM dbo.Menus WITH(UPDLOCK,HOLDLOCK) WHERE Id=@MenuId AND VenueId=@VenueId AND IsPutAway=0;IF @CurrentTargetRevision IS NULL SET @Result=N'target_missing';ELSE BEGIN SELECT @Snapshot=(SELECT m.Id menuId,m.Name name,m.Theme theme,m.DwellSeconds dwellSeconds,m.LoopWarningSeconds loopWarningSeconds,JSON_QUERY((SELECT CAST(a.ScreenId AS nvarchar(36))screenId,a.PageId pageId FROM dbo.MenuScreenAssignments a WITH(UPDLOCK,HOLDLOCK) WHERE a.MenuId=m.Id AND a.VenueId=@VenueId ORDER BY a.ScreenId FOR JSON PATH))screens,JSON_QUERY((SELECT p.Id pageId,p.Name name,p.SortOrder sortOrder FROM dbo.MenuPages p WITH(UPDLOCK,HOLDLOCK) WHERE p.MenuId=m.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))pages,JSON_QUERY((SELECT s.Id sectionId,s.PageId pageId,s.Name name,s.SortOrder sortOrder,JSON_QUERY((SELECT p.ItemId itemId,i.Name name,i.Description description,COALESCE(p.ImportedPriceOverride,i.Price)price,p.ImportedPriceOverride importedPriceOverride,p.SortOrder sortOrder FROM dbo.Placements p WITH(UPDLOCK,HOLDLOCK) JOIN dbo.Items i WITH(UPDLOCK,HOLDLOCK) ON i.Id=p.ItemId AND i.VenueId=p.VenueId WHERE p.MenuSectionId=s.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))items FROM dbo.MenuSections s WITH(UPDLOCK,HOLDLOCK) WHERE s.MenuId=m.Id AND s.VenueId=@VenueId ORDER BY s.SortOrder,s.Id FOR JSON PATH))sections FROM dbo.Menus m WHERE m.Id=@MenuId AND m.VenueId=@VenueId FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);SET @CurrentFingerprint=CONVERT(char(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),@Snapshot)),2);IF @CurrentFingerprint<>@TargetFingerprint SET @Result=N'target_conflict';END END
IF @Result=N'created' BEGIN SELECT TOP(1) @ItemLimit=LimitValue FROM dbo.CapabilityAllowances WITH(UPDLOCK,HOLDLOCK) WHERE CapabilityId=N'content.menu.items' AND(VenueId=@VenueId OR(VenueId IS NULL AND OrganizationId=@OrganizationId))AND StartsUtc<=@Now AND(EndsUtc IS NULL OR EndsUtc>@Now) ORDER BY CASE WHEN VenueId=@VenueId THEN 0 ELSE 1 END,StartsUtc DESC;SELECT TOP(1) @RetentionDays=LimitValue FROM dbo.CapabilityAllowances WITH(UPDLOCK,HOLDLOCK) WHERE CapabilityId=N'content.menu.import.snapshot_retention_days' AND(VenueId=@VenueId OR(VenueId IS NULL AND OrganizationId=@OrganizationId))AND StartsUtc<=@Now AND(EndsUtc IS NULL OR EndsUtc>@Now) ORDER BY CASE WHEN VenueId=@VenueId THEN 0 ELSE 1 END,StartsUtc DESC;END
IF @Result=N'created' BEGIN
 CREATE TABLE #Sections(LineNumber bigint PRIMARY KEY,SectionId uniqueidentifier,Name nvarchar(200),SortOrder int);INSERT #Sections SELECT CONVERT(bigint,LineNumber)*1000+LineSubIndex,NEWID(),ParsedName,ROW_NUMBER()OVER(ORDER BY LineNumber,LineSubIndex)-1 FROM dbo.MenuImportSourceLines WHERE SessionId=@SessionId AND Disposition=N'section';
 CREATE TABLE #Rows(LineNumber bigint PRIMARY KEY,ItemId uniqueidentifier,Existing bit,Fallback bit,Name nvarchar(200),Description nvarchar(1000),Price nvarchar(12),SectionId uniqueidentifier,SortOrder int);INSERT #Rows(LineNumber,ItemId,Existing,Fallback,Name,Description,Price) SELECT CONVERT(bigint,l.LineNumber)*1000+l.LineSubIndex,COALESCE(a.SelectedItemId,NEWID()),IIF(a.SelectedItemId IS NULL,0,1),IIF(l.Disposition=N'unresolved',1,0),IIF(l.Disposition=N'item',l.ParsedName,NULLIF(LTRIM(RTRIM(l.RawText)),N'')),l.ParsedDescription,l.ParsedPrice FROM dbo.MenuImportSourceLines l LEFT JOIN dbo.MenuImportQuestionLines ql ON ql.SessionId=l.SessionId AND ql.LineNumber=l.LineNumber AND l.LineSubIndex=0 LEFT JOIN dbo.MenuImportAnswers a ON a.SessionId=ql.SessionId AND a.QuestionKey=ql.QuestionKey WHERE l.SessionId=@SessionId AND(l.Disposition=N'item' OR(l.Disposition=N'unresolved' AND a.Choice=N'fallback'));
 /* Same rule as the create path: link to the dish the library already holds (migration 082), and
    let rows sharing a name share one new id. See ConfirmCreateSql for the full note. */
 UPDATE r SET ItemId=existing.Id, Existing=1 FROM #Rows r CROSS APPLY (SELECT TOP 1 i.Id FROM dbo.Items i WHERE i.VenueId=@VenueId AND i.IsActive=1 AND i.CanonicalName=dbo.CanonicalItemName(r.Name) ORDER BY i.CreatedUtc,i.Id) existing WHERE r.Existing=0 AND r.Name IS NOT NULL;
 WITH shared AS (SELECT LineNumber, FIRST_VALUE(ItemId) OVER (PARTITION BY dbo.CanonicalItemName(Name) ORDER BY LineNumber) AS Id FROM #Rows WHERE Existing=0) UPDATE r SET ItemId=shared.Id FROM #Rows r JOIN shared ON shared.LineNumber=r.LineNumber WHERE r.Existing=0;
 IF(SELECT COUNT(*)FROM #Rows)>@ItemLimit SET @Result=N'item_limit';ELSE IF EXISTS(SELECT 1 FROM #Rows WHERE Name IS NULL OR LEN(Name)>200)SET @Result=N'invalid_content';
 ELSE BEGIN
  IF EXISTS(SELECT 1 FROM #Rows r WHERE r.Fallback=1 OR NOT EXISTS(SELECT 1 FROM #Sections s WHERE s.LineNumber<r.LineNumber))INSERT #Sections VALUES(0,NEWID(),N'Imported items',-1);UPDATE r SET SectionId=CASE WHEN Fallback=1 THEN(SELECT SectionId FROM #Sections WHERE LineNumber=0)ELSE COALESCE((SELECT TOP 1 SectionId FROM #Sections s WHERE s.LineNumber<r.LineNumber ORDER BY LineNumber DESC),(SELECT SectionId FROM #Sections WHERE LineNumber=0))END FROM #Rows r;WITH x AS(SELECT LineNumber,ROW_NUMBER()OVER(PARTITION BY SectionId ORDER BY LineNumber)-1 n FROM #Rows)UPDATE r SET SortOrder=x.n FROM #Rows r JOIN x ON x.LineNumber=r.LineNumber;
  DELETE FROM dbo.Placements WHERE VenueId=@VenueId AND MenuId=@MenuId;DELETE FROM dbo.MenuSections WHERE VenueId=@VenueId AND MenuId=@MenuId;
  INSERT dbo.MenuSections(Id,VenueId,MenuId,PageId,Name,SortOrder,CreatedUtc,UpdatedUtc)SELECT SectionId,@VenueId,@MenuId,@PageId,Name,ROW_NUMBER()OVER(ORDER BY SortOrder,LineNumber)-1,@Now,@Now FROM #Sections;INSERT dbo.Items(Id,VenueId,Name,Description,Price,Source,IsActive,CreatedUtc,UpdatedUtc)SELECT ItemId,@VenueId,Name,Description,Price,N'import',1,@Now,@Now FROM(SELECT ItemId,Name,Description,Price,ROW_NUMBER()OVER(PARTITION BY ItemId ORDER BY LineNumber)AS Pick FROM #Rows WHERE Existing=0)first WHERE first.Pick=1;INSERT dbo.Placements(Id,VenueId,MenuId,MenuSectionId,PageId,ItemId,SortOrder,CreatedUtc,UpdatedUtc,ImportedPriceOverride)SELECT NEWID(),@VenueId,@MenuId,SectionId,@PageId,ItemId,SortOrder,@Now,@Now,IIF(Existing=1,Price,NULL) FROM(SELECT SectionId,ItemId,SortOrder,Existing,Price,ROW_NUMBER()OVER(PARTITION BY ItemId ORDER BY LineNumber)AS Pick FROM #Rows)once WHERE once.Pick=1;
  UPDATE dbo.Menus SET UpdatedUtc=@Now WHERE Id=@MenuId;
  SELECT @PostSnapshot=(SELECT m.Id menuId,m.Name name,m.Theme theme,m.DwellSeconds dwellSeconds,m.LoopWarningSeconds loopWarningSeconds,JSON_QUERY((SELECT CAST(a.ScreenId AS nvarchar(36))screenId,a.PageId pageId FROM dbo.MenuScreenAssignments a WHERE a.MenuId=m.Id AND a.VenueId=@VenueId ORDER BY a.ScreenId FOR JSON PATH))screens,JSON_QUERY((SELECT p.Id pageId,p.Name name,p.SortOrder sortOrder FROM dbo.MenuPages p WHERE p.MenuId=m.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))pages,JSON_QUERY((SELECT s.Id sectionId,s.PageId pageId,s.Name name,s.SortOrder sortOrder,JSON_QUERY((SELECT p.ItemId itemId,i.Name name,i.Description description,COALESCE(p.ImportedPriceOverride,i.Price)price,p.ImportedPriceOverride importedPriceOverride,p.SortOrder sortOrder FROM dbo.Placements p JOIN dbo.Items i ON i.Id=p.ItemId AND i.VenueId=p.VenueId WHERE p.MenuSectionId=s.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))items FROM dbo.MenuSections s WHERE s.MenuId=m.Id AND s.VenueId=@VenueId ORDER BY s.SortOrder,s.Id FOR JSON PATH))sections FROM dbo.Menus m WHERE m.Id=@MenuId AND m.VenueId=@VenueId FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);SET @PostFingerprint=CONVERT(char(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),@PostSnapshot)),2);
  INSERT dbo.MenuImportReplacementSnapshots(Id,VenueId,MenuId,SessionId,SnapshotJson,CreatedUtc,CreatedBy,ExpiresUtc,ExpectedMenuUpdatedUtc,ExpectedWorkingFingerprint)VALUES(@SnapshotId,@VenueId,@MenuId,@SessionId,@Snapshot,@Now,@Actor,DATEADD(day,@RetentionDays,@Now),@Now,@PostFingerprint);
  UPDATE dbo.MenuImportSessions SET ItemCount=(SELECT COUNT(*)FROM #Rows),CompletedMenuId=@MenuId,CompletedSnapshotId=@SnapshotId,CompletedUtc=@Now,UpdatedUtc=@Now,UpdatedBy=@Actor WHERE Id=@SessionId;
 END
END
IF @Result<>N'created' AND @Result<>N'already_completed' SET @MenuId=NULL;COMMIT;SELECT @Result Result,@MenuId MenuId;
""";

    private const string RestoreReplacementSql = """
SET XACT_ABORT ON;BEGIN TRANSACTION;DECLARE @Result nvarchar(24)=N'restored',@MenuId uniqueidentifier,@Snapshot nvarchar(max),@CurrentSnapshot nvarchar(max),@Expiry datetime2,@Restored datetime2,@ExpectedMenuUpdated datetime2,@CurrentMenuUpdated datetime2,@ExpectedFingerprint char(64),@CurrentFingerprint char(64),@Org uniqueidentifier,@Enabled int=1;
SELECT @MenuId=MenuId,@Snapshot=SnapshotJson,@Expiry=ExpiresUtc,@Restored=RestoredUtc,@ExpectedMenuUpdated=ExpectedMenuUpdatedUtc,@ExpectedFingerprint=ExpectedWorkingFingerprint FROM dbo.MenuImportReplacementSnapshots WITH(UPDLOCK,HOLDLOCK) WHERE Id=@SnapshotId AND VenueId=@VenueId;
IF @MenuId IS NULL SET @Result=N'not_found';ELSE IF @Restored IS NOT NULL SET @Result=N'already_restored';ELSE IF @Expiry<=@Now SET @Result=N'expired';
IF @Result=N'restored' BEGIN SELECT @Org=OrganizationId FROM dbo.Venues WITH(UPDLOCK,HOLDLOCK) WHERE Id=@VenueId;IF NOT EXISTS(SELECT 1 FROM OPENJSON(@SystemRolesJson) r JOIN dbo.AuthorityRolePermissions rp WITH(UPDLOCK,HOLDLOCK) ON rp.RoleKey=r.value AND rp.PermissionId=N'content.menu.import' UNION ALL SELECT 1 FROM dbo.ScopedRoleAssignments a WITH(UPDLOCK,HOLDLOCK) JOIN dbo.AuthorityRolePermissions rp WITH(UPDLOCK,HOLDLOCK) ON rp.RoleKey=a.RoleKey AND rp.PermissionId=N'content.menu.import' WHERE a.ActorUserId=@ActorUserId AND a.RevokedUtc IS NULL AND a.StartsUtc<=@Now AND(a.ExpiresUtc IS NULL OR a.ExpiresUtc>@Now)AND((a.ScopeType=4 AND a.ScopeId=@VenueId)OR(a.ScopeType=2 AND a.ScopeId=@Org)))SET @Result=N'permission_denied';SELECT TOP(1)@Enabled=LimitValue FROM dbo.CapabilityAllowances WITH(UPDLOCK,HOLDLOCK) WHERE CapabilityId=N'content.menu.import.restore_enabled' AND(VenueId=@VenueId OR(VenueId IS NULL AND OrganizationId=@Org))AND StartsUtc<=@Now AND(EndsUtc IS NULL OR EndsUtc>@Now)ORDER BY CASE WHEN VenueId=@VenueId THEN 0 ELSE 1 END,StartsUtc DESC;IF @Enabled=0 SET @Result=N'permission_denied';END
IF @Result=N'restored' BEGIN SELECT @CurrentMenuUpdated=UpdatedUtc FROM dbo.Menus WITH(UPDLOCK,HOLDLOCK) WHERE Id=@MenuId AND VenueId=@VenueId;IF @CurrentMenuUpdated IS NULL SET @Result=N'not_found';ELSE BEGIN SELECT @CurrentSnapshot=(SELECT m.Id menuId,m.Name name,m.Theme theme,m.DwellSeconds dwellSeconds,m.LoopWarningSeconds loopWarningSeconds,JSON_QUERY((SELECT CAST(a.ScreenId AS nvarchar(36))screenId,a.PageId pageId FROM dbo.MenuScreenAssignments a WITH(UPDLOCK,HOLDLOCK) WHERE a.MenuId=m.Id AND a.VenueId=@VenueId ORDER BY a.ScreenId FOR JSON PATH))screens,JSON_QUERY((SELECT p.Id pageId,p.Name name,p.SortOrder sortOrder FROM dbo.MenuPages p WITH(UPDLOCK,HOLDLOCK) WHERE p.MenuId=m.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))pages,JSON_QUERY((SELECT s.Id sectionId,s.PageId pageId,s.Name name,s.SortOrder sortOrder,JSON_QUERY((SELECT p.ItemId itemId,i.Name name,i.Description description,COALESCE(p.ImportedPriceOverride,i.Price)price,p.ImportedPriceOverride importedPriceOverride,p.SortOrder sortOrder FROM dbo.Placements p WITH(UPDLOCK,HOLDLOCK) JOIN dbo.Items i WITH(UPDLOCK,HOLDLOCK) ON i.Id=p.ItemId AND i.VenueId=p.VenueId WHERE p.MenuSectionId=s.Id AND p.VenueId=@VenueId ORDER BY p.SortOrder,p.Id FOR JSON PATH))items FROM dbo.MenuSections s WITH(UPDLOCK,HOLDLOCK) WHERE s.MenuId=m.Id AND s.VenueId=@VenueId ORDER BY s.SortOrder,s.Id FOR JSON PATH))sections FROM dbo.Menus m WHERE m.Id=@MenuId AND m.VenueId=@VenueId FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);SET @CurrentFingerprint=CONVERT(char(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),@CurrentSnapshot)),2);IF @CurrentFingerprint<>@ExpectedFingerprint SET @Result=N'conflict';END END
IF @Result=N'restored' BEGIN
 DECLARE @Pages table(Id uniqueidentifier,Name nvarchar(200),SortOrder int);INSERT @Pages SELECT pageId,name,sortOrder FROM OPENJSON(@Snapshot,'$.pages')WITH(pageId uniqueidentifier '$.pageId',name nvarchar(200),sortOrder int '$.sortOrder');DECLARE @Sections table(Id uniqueidentifier,PageId uniqueidentifier,Name nvarchar(200),SortOrder int,Items nvarchar(max));INSERT @Sections SELECT sectionId,pageId,name,sortOrder,items FROM OPENJSON(@Snapshot,'$.sections')WITH(sectionId uniqueidentifier '$.sectionId',pageId uniqueidentifier '$.pageId',name nvarchar(200),sortOrder int '$.sortOrder',items nvarchar(max)'$.items' AS JSON);
 DELETE FROM dbo.Placements WHERE VenueId=@VenueId AND MenuId=@MenuId;DELETE FROM dbo.MenuSections WHERE VenueId=@VenueId AND MenuId=@MenuId;INSERT dbo.MenuSections(Id,VenueId,MenuId,PageId,Name,SortOrder,CreatedUtc,UpdatedUtc)SELECT Id,@VenueId,@MenuId,PageId,Name,SortOrder,@Now,@Now FROM @Sections;
 INSERT dbo.Placements(Id,VenueId,MenuId,MenuSectionId,PageId,ItemId,SortOrder,CreatedUtc,UpdatedUtc,ImportedPriceOverride)SELECT NEWID(),@VenueId,@MenuId,s.Id,s.PageId,j.itemId,j.sortOrder,@Now,@Now,j.importedPriceOverride FROM @Sections s CROSS APPLY OPENJSON(s.Items)WITH(itemId uniqueidentifier '$.itemId',price nvarchar(12)'$.price',sortOrder int '$.sortOrder',importedPriceOverride nvarchar(12)'$.importedPriceOverride')j;
 UPDATE dbo.Menus SET UpdatedUtc=@Now WHERE Id=@MenuId;UPDATE dbo.MenuImportReplacementSnapshots SET RestoredUtc=@Now,RestoredBy=@Actor WHERE Id=@SnapshotId;INSERT dbo.MenuHistoryEntries(Id,VenueId,MenuId,Kind,Detail,Author,OccurredUtc)VALUES(NEWID(),@VenueId,@MenuId,N'restored',N'Restored the working menu saved before paste replacement.',@Actor,@Now);
END
COMMIT;SELECT @Result Result,@MenuId MenuId;
""";

    private const string AnswerSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION;
DECLARE @Result nvarchar(20)=N'updated';
DECLARE @CurrentRevision varbinary(8), @CurrentExpiry datetime2;
SELECT @CurrentRevision=Revision,@CurrentExpiry=ExpiresUtc FROM dbo.MenuImportSessions WITH (UPDLOCK,HOLDLOCK) WHERE Id=@SessionId AND VenueId=@VenueId;
IF @CurrentRevision IS NULL SET @Result=N'not_found';
ELSE IF @CurrentExpiry<=@Now SET @Result=N'expired';
ELSE IF @CurrentRevision<>@ExpectedRevision SET @Result=N'conflict';
ELSE IF NOT EXISTS(SELECT 1 FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=@SessionId AND q.VenueId=@VenueId AND q.QuestionKey=@QuestionKey AND q.Fingerprint=@Fingerprint AND q.ParseRevision=(SELECT ParseRevision FROM dbo.MenuImportSessions WHERE Id=@SessionId)) SET @Result=N'invalid';
ELSE IF @Choice=N'same_item' AND NOT EXISTS(SELECT 1 FROM dbo.MenuImportCandidates WHERE SessionId=@SessionId AND QuestionKey=@QuestionKey AND ItemId=@SelectedItemId) SET @Result=N'invalid';
IF @Result=N'updated' BEGIN
 MERGE dbo.MenuImportAnswers AS target USING (SELECT @SessionId SessionId, @QuestionKey QuestionKey) source ON target.SessionId=source.SessionId AND target.QuestionKey=source.QuestionKey
 WHEN MATCHED THEN UPDATE SET Fingerprint=@Fingerprint, Choice=@Choice, SelectedItemId=@SelectedItemId, ParseRevision=(SELECT ParseRevision FROM dbo.MenuImportSessions WHERE Id=@SessionId), AnsweredUtc=@Now, AnsweredBy=@Actor
 WHEN NOT MATCHED THEN INSERT(SessionId,VenueId,QuestionKey,Fingerprint,Choice,SelectedItemId,ParseRevision,AnsweredUtc,AnsweredBy) VALUES(@SessionId,@VenueId,@QuestionKey,@Fingerprint,@Choice,@SelectedItemId,(SELECT ParseRevision FROM dbo.MenuImportSessions WHERE Id=@SessionId),@Now,@Actor);
 UPDATE dbo.MenuImportSessions SET UpdatedUtc=@Now, UpdatedBy=@Actor, Status=CASE WHEN NOT EXISTS(SELECT 1 FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=@SessionId AND q.Required=1 AND NOT EXISTS(SELECT 1 FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey AND a.Fingerprint=q.Fingerprint)) THEN N'resolved' ELSE N'reviewing' END WHERE Id=@SessionId;
END
COMMIT; SELECT @Result Result;
""";

    private const string AcceptSafeSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION; DECLARE @Result nvarchar(20)=N'updated';
DECLARE @CurrentRevision varbinary(8), @CurrentExpiry datetime2;
SELECT @CurrentRevision=Revision,@CurrentExpiry=ExpiresUtc FROM dbo.MenuImportSessions WITH (UPDLOCK,HOLDLOCK) WHERE Id=@SessionId AND VenueId=@VenueId;
IF @CurrentRevision IS NULL SET @Result=N'not_found';
ELSE IF @CurrentExpiry<=@Now SET @Result=N'expired';
ELSE IF @CurrentRevision<>@ExpectedRevision SET @Result=N'conflict';
IF @Result=N'updated' BEGIN
 INSERT dbo.MenuImportAnswers(SessionId,VenueId,QuestionKey,Fingerprint,Choice,SelectedItemId,ParseRevision,AnsweredUtc,AnsweredBy)
 SELECT q.SessionId,q.VenueId,q.QuestionKey,q.Fingerprint,N'same_item',MIN(c.ItemId),q.ParseRevision,@Now,@Actor FROM dbo.MenuImportReviewQuestions q JOIN dbo.MenuImportCandidates c ON c.SessionId=q.SessionId AND c.QuestionKey=q.QuestionKey AND c.IsSafe=1
 WHERE q.SessionId=@SessionId AND NOT EXISTS(SELECT 1 FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey)
 GROUP BY q.SessionId,q.VenueId,q.QuestionKey,q.Fingerprint,q.ParseRevision HAVING COUNT(*)=1;
 UPDATE dbo.MenuImportSessions SET UpdatedUtc=@Now,UpdatedBy=@Actor,Status=CASE WHEN NOT EXISTS(SELECT 1 FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=@SessionId AND q.Required=1 AND NOT EXISTS(SELECT 1 FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey AND a.Fingerprint=q.Fingerprint)) THEN N'resolved' ELSE N'reviewing' END WHERE Id=@SessionId;
END COMMIT; SELECT @Result Result;
""";

    private static readonly string ReplaceSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION; DECLARE @Result nvarchar(20)=N'updated';
DECLARE @CurrentRevision varbinary(8), @CurrentExpiry datetime2;
SELECT @CurrentRevision=Revision,@CurrentExpiry=ExpiresUtc FROM dbo.MenuImportSessions WITH (UPDLOCK,HOLDLOCK) WHERE Id=@SessionId AND VenueId=@VenueId;
IF @CurrentRevision IS NULL SET @Result=N'not_found';
ELSE IF @CurrentExpiry<=@Now SET @Result=N'expired';
ELSE IF @CurrentRevision<>@ExpectedRevision SET @Result=N'conflict';
IF @Result=N'updated' BEGIN
 SELECT * INTO #Answers FROM dbo.MenuImportAnswers WHERE SessionId=@SessionId;
 DELETE FROM dbo.MenuImportAnswers WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportCandidates WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportQuestionLines WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportReviewQuestions WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportSourceLines WHERE SessionId=@SessionId;
 UPDATE dbo.MenuImportSessions SET RawPaste=@RawPaste,ParseRevision=@ParseRevision,Status=@Status,LineCount=@LineCount,ItemCount=@ItemCount,ExpiresUtc=@ExpiresUtc,UpdatedUtc=@UpdatedUtc,UpdatedBy=@UpdatedBy WHERE Id=@SessionId;
""" + InsertDerivedSql + """
 -- An operator's own answer outranks one the parser gave itself, so whatever the derived insert
 -- above pre-answered is cleared for exactly the questions being restored. Both land on
 -- PK (SessionId, QuestionKey) otherwise, and a re-parse IS a resume - so it surfaced as
 -- "We couldn't resume this import", error 500.
 DELETE a FROM dbo.MenuImportAnswers a
  WHERE a.SessionId=@SessionId AND EXISTS(SELECT 1 FROM #Answers old JOIN dbo.MenuImportReviewQuestions q ON q.SessionId=old.SessionId AND q.Fingerprint=old.Fingerprint WHERE q.QuestionKey=a.QuestionKey);
 INSERT dbo.MenuImportAnswers(SessionId,VenueId,QuestionKey,Fingerprint,Choice,SelectedItemId,ParseRevision,AnsweredUtc,AnsweredBy)
 SELECT old.SessionId,old.VenueId,q.QuestionKey,q.Fingerprint,old.Choice,old.SelectedItemId,q.ParseRevision,old.AnsweredUtc,old.AnsweredBy FROM #Answers old JOIN dbo.MenuImportReviewQuestions q ON q.SessionId=old.SessionId AND q.Fingerprint=old.Fingerprint
 WHERE old.SelectedItemId IS NULL OR EXISTS(SELECT 1 FROM dbo.MenuImportCandidates c WHERE c.SessionId=q.SessionId AND c.QuestionKey=q.QuestionKey AND c.ItemId=old.SelectedItemId);
 UPDATE dbo.MenuImportSessions SET Status=CASE WHEN NOT EXISTS(SELECT 1 FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=@SessionId AND q.Required=1 AND NOT EXISTS(SELECT 1 FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey AND a.Fingerprint=q.Fingerprint)) THEN N'resolved' ELSE N'reviewing' END WHERE Id=@SessionId;
END COMMIT; SELECT @Result Result;
""";

    /// <summary>
    /// The venue's unfinished imports. Completed ones are excluded: their work is on a menu, and
    /// offering to "resume" one would send the operator back to a review with nothing left to do.
    ///
    /// AnswersRemaining is counted at the session's CURRENT parse revision. A re-parse retires the
    /// old questions, and counting those would report work that no longer exists.
    /// </summary>
    private const string ListOpenSql = """
SELECT s.Id, s.ItemCount, s.LineCount, s.CreatedUtc, s.UpdatedUtc, s.ExpiresUtc,
       (SELECT COUNT(*) FROM dbo.MenuImportReviewQuestions q
        WHERE q.SessionId=s.Id AND q.VenueId=s.VenueId AND q.Required=1 AND q.ParseRevision=s.ParseRevision
          AND NOT EXISTS (SELECT 1 FROM dbo.MenuImportAnswers a
              WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey AND a.Fingerprint=q.Fingerprint)) AS AnswersRemaining
FROM dbo.MenuImportSessions s
WHERE s.VenueId=@VenueId AND s.ExpiresUtc>@Now AND s.CompletedMenuId IS NULL
ORDER BY s.UpdatedUtc DESC;
""";

    /// <summary>
    /// Throwing away an unfinished import, in the same order the expiry sweep uses.
    ///
    /// A completed session is deliberately out of reach here: its replacement snapshot is what an
    /// operator restores a replaced menu from, and deleting the session would take the way back
    /// with it. Those are the sweeper's to remove once they expire.
    /// </summary>
    private const string DiscardSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION;
DECLARE @Live TABLE (Id uniqueidentifier PRIMARY KEY);
INSERT @Live SELECT Id FROM dbo.MenuImportSessions WITH (UPDLOCK, HOLDLOCK)
WHERE Id=@SessionId AND VenueId=@VenueId AND CompletedMenuId IS NULL;
DELETE cl FROM dbo.MenuImportCreatedLines cl JOIN @Live s ON s.Id=cl.SessionId;
DELETE a FROM dbo.MenuImportAnswers a JOIN @Live s ON s.Id=a.SessionId;
DELETE c FROM dbo.MenuImportCandidates c JOIN @Live s ON s.Id=c.SessionId;
DELETE ql FROM dbo.MenuImportQuestionLines ql JOIN @Live s ON s.Id=ql.SessionId;
DELETE q FROM dbo.MenuImportReviewQuestions q JOIN @Live s ON s.Id=q.SessionId;
DELETE l FROM dbo.MenuImportSourceLines l JOIN @Live s ON s.Id=l.SessionId;
DELETE session FROM dbo.MenuImportSessions session JOIN @Live s ON s.Id=session.Id;
DECLARE @Count int=@@ROWCOUNT; COMMIT; SELECT @Count [Count];
""";

    private sealed class SummaryRow
    {
        public Guid Id { get; init; }
        public int ItemCount { get; init; }
        public int LineCount { get; init; }
        public int AnswersRemaining { get; init; }
        public DateTime CreatedUtc { get; init; }
        public DateTime UpdatedUtc { get; init; }
        public DateTime ExpiresUtc { get; init; }
    }

    private const string DeleteExpiredSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION; CREATE TABLE #Sessions(Id uniqueidentifier PRIMARY KEY);
INSERT #Sessions SELECT TOP (@BatchSize) Id FROM dbo.MenuImportSessions WITH (UPDLOCK, READPAST) WHERE ExpiresUtc<=@Now ORDER BY ExpiresUtc;
DELETE cl FROM dbo.MenuImportCreatedLines cl JOIN #Sessions s ON s.Id=cl.SessionId; DELETE a FROM dbo.MenuImportAnswers a JOIN #Sessions s ON s.Id=a.SessionId; DELETE c FROM dbo.MenuImportCandidates c JOIN #Sessions s ON s.Id=c.SessionId; DELETE ql FROM dbo.MenuImportQuestionLines ql JOIN #Sessions s ON s.Id=ql.SessionId; DELETE q FROM dbo.MenuImportReviewQuestions q JOIN #Sessions s ON s.Id=q.SessionId; DELETE l FROM dbo.MenuImportSourceLines l JOIN #Sessions s ON s.Id=l.SessionId; DELETE session FROM dbo.MenuImportSessions session JOIN #Sessions s ON s.Id=session.Id;
DECLARE @Count int=@@ROWCOUNT; COMMIT; SELECT @Count [Count];
""";
}
