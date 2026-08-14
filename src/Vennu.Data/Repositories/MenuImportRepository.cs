using System.Text.Json;
using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class MenuImportRepository(ISqlDataAccess dataAccess) : IMenuImportRepository
{
    public async Task<MenuImportAggregate> CreateAsync(MenuImportAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ValidateAggregate(aggregate);
        _ = (await dataAccess.ExecuteSqlQueryAsync<ResultRow, object>(CreateSql, Parameters(aggregate), cancellationToken)
            .ConfigureAwait(false)).Single();
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
        Now = aggregate.Session.UpdatedUtc,
        LinesJson = JsonSerializer.Serialize(aggregate.Lines),
        QuestionsJson = JsonSerializer.Serialize(aggregate.Questions.Select(q => new QuestionPayload(q.QuestionKey, q.Fingerprint, q.Kind, q.DisplayOrder, q.Required, q.ParseRevision))),
        QuestionLinesJson = JsonSerializer.Serialize(aggregate.Questions.SelectMany(q => q.LineNumbers.Select(line => new QuestionLinePayload(q.QuestionKey, line)))),
        CandidatesJson = JsonSerializer.Serialize(aggregate.Questions.SelectMany(q => q.Candidates.Select(c => new CandidatePayload(q.QuestionKey, c.ItemId, c.DisplayName, c.DisplayPrice, c.MatchRule, c.IsSafe))))
    };

    private static MenuImportAggregate Hydrate(AggregateRow row)
    {
        var lines = JsonSerializer.Deserialize<List<MenuImportSourceLine>>(row.LinesJson, JsonOptions) ?? [];
        var questions = JsonSerializer.Deserialize<List<QuestionRead>>(row.QuestionsJson, JsonOptions) ?? [];
        var session = new MenuImportSession(row.Id, row.VenueId, row.RawPaste, row.ParseRevision, row.Status, row.LineCount,
            row.ItemCount, row.ExpiresUtc, row.CreatedUtc, row.UpdatedUtc, row.UpdatedBy, row.Revision);
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
    private static string RequireChoice(string value) => value is MenuImportChoices.SameItem or MenuImportChoices.NewItem or MenuImportChoices.Section or MenuImportChoices.Fallback ? value : throw new ArgumentOutOfRangeException(nameof(value));
    private static string RequireAnswerShape(string choice, Guid? selectedItemId)
    {
        RequireChoice(choice);
        if ((choice == MenuImportChoices.SameItem) != selectedItemId.HasValue)
            throw new ArgumentException("Same-item answers require a selected item; other answers cannot select one.", nameof(selectedItemId));
        return choice;
    }
    private static string RequireFingerprint(string value) => value is { Length: 64 } ? value : throw new ArgumentException("A 64-character fingerprint is required.", nameof(value));

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private sealed record QuestionPayload(string QuestionKey, string Fingerprint, string Kind, int DisplayOrder, bool Required, long ParseRevision);
    private sealed record QuestionLinePayload(string QuestionKey, int LineNumber);
    private sealed record CandidatePayload(string QuestionKey, Guid ItemId, string DisplayName, string? DisplayPrice, string MatchRule, bool IsSafe);
    private sealed record QuestionRead(string QuestionKey, string Fingerprint, string Kind, int DisplayOrder, bool Required, long ParseRevision,
        IReadOnlyCollection<int>? LineNumbers, IReadOnlyCollection<MenuImportCandidate>? Candidates, MenuImportAnswer? Answer);
    private sealed record ResultRow(string Result);
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
        public byte[] Revision { get; init; } = [];
        public string LinesJson { get; init; } = "[]";
        public string QuestionsJson { get; init; } = "[]";
    }

    private const string InsertDerivedSql = """
INSERT dbo.MenuImportSourceLines (SessionId, VenueId, LineNumber, RawText, Disposition, ParsedName, ParsedDescription, ParsedPrice, ParserReason, ParseRevision)
SELECT @SessionId, @VenueId, LineNumber, RawText, Disposition, ParsedName, ParsedDescription, ParsedPrice, ParserReason, ParseRevision
FROM OPENJSON(@LinesJson) WITH (LineNumber int, RawText nvarchar(max), Disposition nvarchar(24), ParsedName nvarchar(200), ParsedDescription nvarchar(1000), ParsedPrice nvarchar(12), ParserReason nvarchar(80), ParseRevision bigint);
INSERT dbo.MenuImportReviewQuestions (SessionId, VenueId, QuestionKey, Fingerprint, Kind, DisplayOrder, Required, ParseRevision)
SELECT @SessionId, @VenueId, QuestionKey, Fingerprint, Kind, DisplayOrder, Required, ParseRevision
FROM OPENJSON(@QuestionsJson) WITH (QuestionKey nvarchar(80), Fingerprint char(64), Kind nvarchar(32), DisplayOrder int, Required bit, ParseRevision bigint);
INSERT dbo.MenuImportQuestionLines (SessionId, VenueId, QuestionKey, LineNumber)
SELECT @SessionId, @VenueId, QuestionKey, LineNumber FROM OPENJSON(@QuestionLinesJson) WITH (QuestionKey nvarchar(80), LineNumber int);
INSERT dbo.MenuImportCandidates (SessionId, VenueId, QuestionKey, ItemId, DisplayName, DisplayPrice, MatchRule, IsSafe)
SELECT @SessionId, @VenueId, QuestionKey, ItemId, DisplayName, DisplayPrice, MatchRule, IsSafe
FROM OPENJSON(@CandidatesJson) WITH (QuestionKey nvarchar(80), ItemId uniqueidentifier, DisplayName nvarchar(200), DisplayPrice nvarchar(12), MatchRule nvarchar(32), IsSafe bit);
""";

    private static readonly string CreateSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION;
INSERT dbo.MenuImportSessions (Id, VenueId, RawPaste, ParseRevision, Status, LineCount, ItemCount, ExpiresUtc, CreatedUtc, UpdatedUtc, UpdatedBy)
VALUES (@SessionId, @VenueId, @RawPaste, @ParseRevision, @Status, @LineCount, @ItemCount, @ExpiresUtc, @CreatedUtc, @UpdatedUtc, @UpdatedBy);
""" + InsertDerivedSql + "COMMIT; SELECT N'updated' Result;";

    private const string ReadSql = """
SELECT s.Id, s.VenueId, s.RawPaste, s.ParseRevision, s.Status, s.LineCount, s.ItemCount, s.ExpiresUtc, s.CreatedUtc, s.UpdatedUtc, s.UpdatedBy, s.Revision,
 (SELECT l.SessionId, l.VenueId, l.LineNumber, l.RawText, l.Disposition, l.ParsedName, l.ParsedDescription, l.ParsedPrice, l.ParserReason, l.ParseRevision FROM dbo.MenuImportSourceLines l WHERE l.SessionId=s.Id ORDER BY l.LineNumber FOR JSON PATH) LinesJson,
 (SELECT q.QuestionKey, q.Fingerprint, q.Kind, q.DisplayOrder, q.Required, q.ParseRevision,
   JSON_QUERY(COALESCE((SELECT N'['+STRING_AGG(CONVERT(nvarchar(max), ql.LineNumber),N',') WITHIN GROUP (ORDER BY ql.LineNumber)+N']' FROM dbo.MenuImportQuestionLines ql WHERE ql.SessionId=q.SessionId AND ql.QuestionKey=q.QuestionKey),N'[]')) LineNumbers,
   JSON_QUERY((SELECT c.ItemId, c.DisplayName, c.DisplayPrice, c.MatchRule, c.IsSafe FROM dbo.MenuImportCandidates c WHERE c.SessionId=q.SessionId AND c.QuestionKey=q.QuestionKey ORDER BY c.DisplayName, c.ItemId FOR JSON PATH)) Candidates,
   JSON_QUERY((SELECT a.Fingerprint, a.Choice, a.SelectedItemId, a.ParseRevision, a.AnsweredUtc, a.AnsweredBy FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)) Answer
  FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=s.Id ORDER BY q.DisplayOrder, q.QuestionKey FOR JSON PATH) QuestionsJson
FROM dbo.MenuImportSessions s WHERE s.Id=@SessionId AND s.VenueId=@VenueId AND s.ExpiresUtc>@Now;
""";

    private const string AnswerSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION;
DECLARE @Result nvarchar(20)=N'updated';
IF NOT EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId) SET @Result=N'not_found';
ELSE IF EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId AND ExpiresUtc<=@Now) SET @Result=N'expired';
ELSE IF NOT EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId AND Revision=@ExpectedRevision) SET @Result=N'conflict';
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
IF NOT EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId) SET @Result=N'not_found';
ELSE IF EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId AND ExpiresUtc<=@Now) SET @Result=N'expired';
ELSE IF NOT EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId AND Revision=@ExpectedRevision) SET @Result=N'conflict';
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
IF NOT EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId) SET @Result=N'not_found';
ELSE IF EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId AND ExpiresUtc<=@Now) SET @Result=N'expired';
ELSE IF NOT EXISTS(SELECT 1 FROM dbo.MenuImportSessions WHERE Id=@SessionId AND VenueId=@VenueId AND Revision=@ExpectedRevision) SET @Result=N'conflict';
IF @Result=N'updated' BEGIN
 SELECT * INTO #Answers FROM dbo.MenuImportAnswers WHERE SessionId=@SessionId;
 DELETE FROM dbo.MenuImportAnswers WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportCandidates WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportQuestionLines WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportReviewQuestions WHERE SessionId=@SessionId; DELETE FROM dbo.MenuImportSourceLines WHERE SessionId=@SessionId;
 UPDATE dbo.MenuImportSessions SET RawPaste=@RawPaste,ParseRevision=@ParseRevision,Status=@Status,LineCount=@LineCount,ItemCount=@ItemCount,ExpiresUtc=@ExpiresUtc,UpdatedUtc=@UpdatedUtc,UpdatedBy=@UpdatedBy WHERE Id=@SessionId;
""" + InsertDerivedSql + """
 INSERT dbo.MenuImportAnswers(SessionId,VenueId,QuestionKey,Fingerprint,Choice,SelectedItemId,ParseRevision,AnsweredUtc,AnsweredBy)
 SELECT old.SessionId,old.VenueId,q.QuestionKey,q.Fingerprint,old.Choice,old.SelectedItemId,q.ParseRevision,old.AnsweredUtc,old.AnsweredBy FROM #Answers old JOIN dbo.MenuImportReviewQuestions q ON q.SessionId=old.SessionId AND q.Fingerprint=old.Fingerprint
 WHERE old.SelectedItemId IS NULL OR EXISTS(SELECT 1 FROM dbo.MenuImportCandidates c WHERE c.SessionId=q.SessionId AND c.QuestionKey=q.QuestionKey AND c.ItemId=old.SelectedItemId);
 UPDATE dbo.MenuImportSessions SET Status=CASE WHEN NOT EXISTS(SELECT 1 FROM dbo.MenuImportReviewQuestions q WHERE q.SessionId=@SessionId AND q.Required=1 AND NOT EXISTS(SELECT 1 FROM dbo.MenuImportAnswers a WHERE a.SessionId=q.SessionId AND a.QuestionKey=q.QuestionKey AND a.Fingerprint=q.Fingerprint)) THEN N'resolved' ELSE N'reviewing' END WHERE Id=@SessionId;
END COMMIT; SELECT @Result Result;
""";

    private const string DeleteExpiredSql = """
SET XACT_ABORT ON; BEGIN TRANSACTION; CREATE TABLE #Sessions(Id uniqueidentifier PRIMARY KEY);
INSERT #Sessions SELECT TOP (@BatchSize) Id FROM dbo.MenuImportSessions WITH (UPDLOCK, READPAST) WHERE ExpiresUtc<=@Now ORDER BY ExpiresUtc;
DELETE a FROM dbo.MenuImportAnswers a JOIN #Sessions s ON s.Id=a.SessionId; DELETE c FROM dbo.MenuImportCandidates c JOIN #Sessions s ON s.Id=c.SessionId; DELETE ql FROM dbo.MenuImportQuestionLines ql JOIN #Sessions s ON s.Id=ql.SessionId; DELETE q FROM dbo.MenuImportReviewQuestions q JOIN #Sessions s ON s.Id=q.SessionId; DELETE l FROM dbo.MenuImportSourceLines l JOIN #Sessions s ON s.Id=l.SessionId; DELETE session FROM dbo.MenuImportSessions session JOIN #Sessions s ON s.Id=session.Id;
DECLARE @Count int=@@ROWCOUNT; COMMIT; SELECT @Count [Count];
""";
}
