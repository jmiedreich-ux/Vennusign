using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;

namespace Vennu.Data.IntegrationTests.Fixtures;

/// <summary>
/// Decides which database an integration run talks to, and how it authenticates.
///
/// <para>
/// Two targets, and the credentials follow from the target rather than being
/// configured alongside it. <b>LocalDB</b> is the default and needs no user and no
/// password - it is integrated security against a database on this machine that the
/// run owns. <b>Azure</b> is opted into for a run, and its credentials come from
/// Key Vault, never from a connection string somebody typed.
/// </para>
///
/// <para>
/// The variable this replaces, <c>VENU_TEST_AZURE_SQL_CONNECTION_STRING</c>, carried
/// a whole connection string with the password inside it. That is how a stale
/// <c>sqladmin</c> password pointed every local run at the dev product database and
/// produced 115 failures reading as product breakage - twice, weeks apart, because
/// a value like that outlives every attempt to remove it. It is now ignored rather
/// than honoured, loudly, because as of 2026-08-21 it is still present in the
/// environment of Windows processes launched from a WSL instance that started
/// before it was deleted from the registry.
/// </para>
/// </summary>
internal static class TestDatabaseTarget
{
    internal const string TargetVariable = "VENU_TEST_TARGET";
    internal const string VaultVariable = "VENU_TEST_AZURE_KEY_VAULT";
    internal const string RetiredConnectionStringVariable = "VENU_TEST_AZURE_SQL_CONNECTION_STRING";

    internal const string DefaultVaultName = "kv-vennusign-dev";

    /// <summary>
    /// Where these tests run unless someone deliberately says otherwise: a local
    /// database, on this machine, that the run owns. No user, no password.
    /// </summary>
    internal const string LocalDbConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=vennusign_dev_tests;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

    internal static bool WantsAzure(string? target) =>
        string.Equals(target?.Trim(), "azure", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The warning printed when the retired variable is still set. Returns null when
    /// it is not, so the caller says nothing in the normal case.
    /// </summary>
    internal static string? RetiredVariableWarning(string? retiredValue, string? target)
    {
        if (string.IsNullOrWhiteSpace(retiredValue))
        {
            return null;
        }

        var replacement = WantsAzure(target)
            ? "It is being ignored; the Azure credentials come from Key Vault."
            : $"It is being ignored and this run uses LocalDB. To target Azure, set {TargetVariable}=azure.";

        return $"[integration] {RetiredConnectionStringVariable} is set and is no longer supported. {replacement} "
             + "If you did not set it, it is inherited: it survives in the environment of Windows processes "
             + "launched from a WSL instance that started before it was deleted. `wsl --shutdown` clears it.";
    }

    /// <summary>
    /// Builds the Azure connection string from the vault, so the password exists only
    /// in memory for the length of the run.
    /// </summary>
    internal static string ResolveAzureConnectionString(string vaultName)
    {
        var client = new SecretClient(new Uri($"https://{vaultName}.vault.azure.net/"), new DefaultAzureCredential());

        string Read(string name)
        {
            try
            {
                var value = client.GetSecret(name).Value.Value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException($"Secret '{name}' in '{vaultName}' is empty.");
                }

                return value;
            }
            catch (Exception exception) when (exception is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    $"Could not read secret '{name}' from '{vaultName}'. The test host authenticates with "
                    + "DefaultAzureCredential, which picks up an `az login` on the same OS as the test host - "
                    + "so the usual cause is that the host cannot see the CLI rather than that there is no "
                    + "sign-in: a Windows test host does not inherit an `az login` performed inside WSL. "
                    + "Either sign in on the host's own side, or run the tests from the side already signed in. "
                    + $"Unset {TargetVariable} to run against LocalDB instead.", exception);
            }
        }

        return new SqlConnectionStringBuilder
        {
            DataSource = $"tcp:{Read("sql-dev-server")},1433",
            InitialCatalog = Read("sql-dev-database"),
            UserID = Read("sql-dev-username"),
            Password = Read("sql-dev-password"),
            Encrypt = true,
            TrustServerCertificate = false,
            ConnectTimeout = 60
        }.ConnectionString;
    }

    internal static string VaultName(string? configured) =>
        string.IsNullOrWhiteSpace(configured) ? DefaultVaultName : configured.Trim();
}
