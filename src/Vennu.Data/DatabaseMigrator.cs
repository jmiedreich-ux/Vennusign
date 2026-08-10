using DbUp;
using System.Reflection;

namespace Vennu.Data;

public static class DatabaseMigrator
{
    public static void Run(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        EnsureDatabase.For.SqlDatabase(connectionString);
        BaselineExistingDatabase(connectionString);

        var assembly = typeof(DatabaseMigrator).Assembly;
        var scriptNames = GetEmbeddedScriptNames(assembly);

        if (scriptNames.Length == 0)
        {
            var availableResources = assembly
                .GetManifestResourceNames()
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .DefaultIfEmpty("(none)");

            throw new InvalidOperationException($"No embedded SQL migration scripts were found in assembly '{assembly.GetName().Name}'. Available manifest resources: {string.Join(", ", availableResources)}");
        }

        Console.WriteLine($"Discovered {scriptNames.Length} embedded SQL migration script(s): {string.Join(", ", scriptNames)}");

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(assembly, name => scriptNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            .WithTransactionPerScript()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw result.Error ?? new InvalidOperationException("Database migration failed.");
        }
    }

    /// <summary>Name of the script that replaced the original fifty-nine.</summary>
    internal const string BaselineScriptName = "Vennu.Data.Scripts.001_baseline.sql";

    /// <summary>The last script of the chain the baseline replaced.</summary>
    private const string FinalSupersededScriptName = "058_create_menu_item_library_spine.sql";

    /// <summary>
    /// Stops the baseline running against a database that already has the schema.
    ///
    /// Deleting a migration never un-applies it: DbUp decides what to run by journal
    /// name, so a database that ran the original chain would see the baseline as new
    /// work and fail on its first CREATE TABLE. Such a database is already at the
    /// baseline's end state, so the baseline is recorded as applied and nothing is
    /// executed against it.
    ///
    /// A database part-way through the old chain is refused rather than guessed at.
    /// Marking it complete would leave it permanently short of whatever it had not
    /// reached, and the gap would only surface later as a missing table.
    /// </summary>
    private static void BaselineExistingDatabase(string connectionString)
    {
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        connection.Open();

        using (var journalExists = new Microsoft.Data.SqlClient.SqlCommand(
            "SELECT OBJECT_ID('dbo.SchemaVersions', 'U');", connection))
        {
            if (journalExists.ExecuteScalar() is null or DBNull)
            {
                // A new database. The baseline runs as the first migration.
                return;
            }
        }

        int supersededCount;
        bool chainComplete;
        bool alreadyBaselined;

        using (var inspect = new Microsoft.Data.SqlClient.SqlCommand(
            """
            SELECT
                SUM(CASE WHEN ScriptName LIKE '%.Scripts.0%' AND ScriptName <> @Baseline THEN 1 ELSE 0 END),
                MAX(CASE WHEN ScriptName LIKE '%' + @Final THEN 1 ELSE 0 END),
                MAX(CASE WHEN ScriptName = @Baseline THEN 1 ELSE 0 END)
            FROM dbo.SchemaVersions;
            """, connection))
        {
            _ = inspect.Parameters.AddWithValue("@Baseline", BaselineScriptName);
            _ = inspect.Parameters.AddWithValue("@Final", FinalSupersededScriptName);
            using var reader = inspect.ExecuteReader();
            if (!reader.Read())
            {
                return;
            }

            supersededCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
            chainComplete = !reader.IsDBNull(1) && reader.GetInt32(1) == 1;
            alreadyBaselined = !reader.IsDBNull(2) && reader.GetInt32(2) == 1;
        }

        if (alreadyBaselined || supersededCount == 0)
        {
            return;
        }

        if (!chainComplete)
        {
            throw new InvalidOperationException(
                $"This database ran {supersededCount} of the migrations the baseline replaced but never reached "
                + $"'{FinalSupersededScriptName}', so it is part-way through a chain that no longer exists. "
                + "Bring it up to date on the previous release first, then deploy this one.");
        }

        // Startup calls this more than once, and can call it concurrently. Checking and
        // then writing let several callers all decide to write: the first database this
        // met ended up with seven identical rows, and a lock hint on the check alone
        // still allowed two. DbUp's journal has no unique constraint to lean on, so the
        // whole decision is serialised behind a named application lock - one caller
        // decides, the rest find the row already there.
        using var record = new Microsoft.Data.SqlClient.SqlCommand(
            """
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

            DECLARE @Acquired INT;
            EXEC @Acquired = sys.sp_getapplock
                @Resource = 'vennusign.schema.baseline',
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 30000;

            IF @Acquired < 0
            BEGIN
                ROLLBACK TRANSACTION;
                THROW 51100, 'Timed out waiting to record the schema baseline; another process holds the lock.', 1;
            END;

            INSERT dbo.SchemaVersions (ScriptName, Applied)
            SELECT @Baseline, SYSUTCDATETIME()
            WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaVersions WHERE ScriptName = @Baseline);

            COMMIT TRANSACTION;
            """,
            connection);
        _ = record.Parameters.AddWithValue("@Baseline", BaselineScriptName);

        if (record.ExecuteNonQuery() > 0)
        {
            Console.WriteLine(
                $"This database already carries the schema the baseline describes ({supersededCount} superseded "
                + "migrations found). Recorded the baseline as applied; nothing was executed against it.");
        }
    }

    internal static string[] GetEmbeddedScriptNames()
    {
        return GetEmbeddedScriptNames(typeof(DatabaseMigrator).Assembly);
    }

    internal static string[] GetEmbeddedScriptNames(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly
            .GetManifestResourceNames()
            .Where(name => name.Contains(".Scripts.", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
