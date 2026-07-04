using DbUp;
using System.Reflection;

namespace Vennu.Data;

public static class DatabaseMigrator
{
    public static void Run(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        EnsureDatabase.For.SqlDatabase(connectionString);

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
