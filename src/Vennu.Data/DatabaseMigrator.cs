using DbUp;
using System.Reflection;

namespace Vennu.Data;

public static class DatabaseMigrator
{
    public static void Run(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        EnsureDatabase.For.SqlDatabase(connectionString);

        var assembly = Assembly.GetExecutingAssembly();
        var scriptNames = assembly
            .GetManifestResourceNames()
            .Where(name => name.Contains(".Scripts.", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (scriptNames.Length == 0)
        {
            return;
        }

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
}
