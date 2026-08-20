using Vennu.Data;

namespace Vennu.Api.Tests;

/// <summary>
/// /health/version is anonymous and public, so what it costs per request matters.
/// These cover the cache and the naming, not the database read - that is covered
/// against a real database in Vennu.Data.IntegrationTests.
/// </summary>
public sealed class DatabaseSchemaVersionCachingTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void NoConnectionStringReportsUnavailableRatherThanThrowing()
    {
        Assert.Equal(DatabaseSchemaVersion.Unavailable, new DatabaseSchemaVersion(null).Current());
        Assert.Equal(DatabaseSchemaVersion.Unavailable, new DatabaseSchemaVersion("   ").Current());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TheJournalResourceNameIsReportedAsTheScriptOnDisk()
    {
        Assert.Equal(
            "073_customer_onboarding_go_live_achieved",
            DatabaseSchemaVersion.Describe("Vennu.Data.Scripts.073_customer_onboarding_go_live_achieved.sql"));

        // A journal written by something other than this assembly's naming.
        Assert.Equal("073_something", DatabaseSchemaVersion.Describe("073_something.sql"));
        Assert.Equal("073_something", DatabaseSchemaVersion.Describe("  073_something  "));

        Assert.Equal(DatabaseSchemaVersion.Unavailable, DatabaseSchemaVersion.Describe(null));
        Assert.Equal(DatabaseSchemaVersion.Unavailable, DatabaseSchemaVersion.Describe(""));
        Assert.Equal(DatabaseSchemaVersion.Unavailable, DatabaseSchemaVersion.Describe("Vennu.Data.Scripts..sql"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AnUnreachableDatabaseIsNotRetriedOnEveryRequest()
    {
        var time = new StoppedClock(DateTimeOffset.Parse("2026-08-20T00:00:00Z"));
        var reader = new CountingSchemaVersion("unreachable", time);

        Assert.Equal(DatabaseSchemaVersion.Unavailable, reader.Current());
        Assert.Equal(DatabaseSchemaVersion.Unavailable, reader.Current());
        Assert.Equal(DatabaseSchemaVersion.Unavailable, reader.Current());

        // Three requests, one read. Without the cache an anonymous caller in a loop
        // opens a database connection per request.
        Assert.Equal(1, reader.Reads);

        time.Advance(TimeSpan.FromSeconds(31));
        Assert.Equal(DatabaseSchemaVersion.Unavailable, reader.Current());
        Assert.Equal(2, reader.Reads);
    }

    private sealed class StoppedClock(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    /// <summary>
    /// A connection string that cannot resolve, so Current() takes the failure path
    /// without needing a database - the point under test is how often it is taken.
    /// </summary>
    private sealed class CountingSchemaVersion(string connectionString, TimeProvider timeProvider)
        : DatabaseSchemaVersion(connectionString, timeProvider)
    {
        public int Reads { get; private set; }

        protected override string ReadFromDatabase()
        {
            Reads++;
            return base.ReadFromDatabase();
        }
    }
}
