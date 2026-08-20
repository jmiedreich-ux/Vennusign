using Microsoft.Data.SqlClient;

namespace Vennu.Data;

/// <summary>
/// Reports the schema the database is actually at, read from DbUp's journal.
///
/// <para>
/// Every other field on <c>/health/version</c> is a build asserting something about
/// itself. This one is the only field that asks the database. That matters because
/// the two can disagree: the build is what was deployed, the schema is what the
/// deployed build managed to apply. Reporting a value the deploy job simply handed
/// in - which is what <c>VENNU_DATABASE_SCHEMA_VERSION</c> did - cannot ever show
/// that disagreement, because it is the deploy talking about itself again.
/// </para>
///
/// <para>
/// This is a second opinion, not the primary deploy check. A build that is running
/// has already migrated, because <see cref="DatabaseMigrator"/> runs before the
/// host is built and throws rather than continuing; an API answering requests on an
/// un-migrated database is not a state this code can reach. What this catches is
/// the case outside that reasoning: a database rolled back, restored, or pointed at
/// a different server underneath a build that is already up.
/// </para>
///
/// <para>
/// It is read behind a short cache because <c>/health/version</c> is anonymous and
/// public, and an uncached read would let any caller open a database connection per
/// request. Staleness is harmless here: the deployment check matches on the commit,
/// not on this.
/// </para>
/// </summary>
public class DatabaseSchemaVersion
{
    /// <summary>What is reported when the database cannot answer.</summary>
    public const string Unavailable = "unavailable";

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly string? _connectionString;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _gate = new();
    private string? _cached;
    private DateTimeOffset _cachedAtUtc;

    public DatabaseSchemaVersion(string? connectionString, TimeProvider? timeProvider = null)
    {
        _connectionString = connectionString;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string Current()
    {
        var now = _timeProvider.GetUtcNow();

        lock (_gate)
        {
            if (_cached is not null && now - _cachedAtUtc < CacheDuration)
            {
                return _cached;
            }
        }

        var value = ReadFromDatabase();

        lock (_gate)
        {
            _cached = value;
            _cachedAtUtc = now;
        }

        return value;
    }

    /// <summary>Virtual so a test can observe how often this is reached.</summary>
    protected virtual string ReadFromDatabase()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return Unavailable;
        }

        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Ordering by name rather than by Applied: the question is what schema
            // level the database is at, and a script re-applied out of order would
            // still leave the level where the highest-numbered script put it. The
            // numbers are zero-padded, so lexical order is numeric order.
            using var command = new SqlCommand(
                "SELECT TOP 1 ScriptName FROM dbo.SchemaVersions ORDER BY ScriptName DESC;",
                connection)
            {
                CommandTimeout = 5
            };

            return Describe(command.ExecuteScalar() as string);
        }
        catch (Exception)
        {
            // Deliberately total. This value is reported by a health endpoint that
            // has to keep answering when the database does not - an unreachable
            // database is a fact to report, not a reason to fail the request. The
            // exception itself is not surfaced: the endpoint is anonymous and public,
            // and #730 is already an open defect about a stack trace reaching one.
            return Unavailable;
        }
    }

    /// <summary>
    /// Turns the journal's resource name into the script a person would look for.
    /// DbUp records "Vennu.Data.Scripts.073_customer_onboarding_go_live_achieved.sql";
    /// the file on disk is "073_customer_onboarding_go_live_achieved.sql".
    /// </summary>
    internal static string Describe(string? scriptName)
    {
        if (string.IsNullOrWhiteSpace(scriptName))
        {
            return Unavailable;
        }

        const string prefix = "Vennu.Data.Scripts.";
        var value = scriptName.Trim();

        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = value[prefix.Length..];
        }

        if (value.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
        {
            value = value[..^".sql".Length];
        }

        return value.Length == 0 ? Unavailable : value;
    }
}
