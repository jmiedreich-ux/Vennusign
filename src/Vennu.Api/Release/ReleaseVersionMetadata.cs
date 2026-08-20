namespace Vennu.Api.Release;

public sealed record ReleaseVersionMetadata(
    string ProductVersion,
    string ComponentVersion,
    int ApiContractMajor,
    string SourceCommit,
    string BuildId,
    string DatabaseSchemaVersion,
    string ConfigurationSchemaVersion)
{
    /// <param name="databaseSchemaVersion">
    /// What the database says its schema is. Passed in rather than read from the
    /// environment because a deploy-supplied value can only ever repeat what the
    /// deploy believed; see <see cref="Vennu.Data.DatabaseSchemaVersion"/>. Null
    /// keeps the old environment-variable behaviour for callers that have no
    /// database to ask - tests, and the local defaults.
    /// </param>
    public static ReleaseVersionMetadata FromEnvironment(string? databaseSchemaVersion = null) => new(
        Value("VENNU_PRODUCT_VERSION", "0.0.0-local"),
        Value("VENNU_COMPONENT_VERSION", "0.0.0-local"),
        PositiveInteger("VENNU_API_CONTRACT_MAJOR", 1),
        Value("VENNU_SOURCE_COMMIT", "local"),
        Value("VENNU_BUILD_ID", "local"),
        string.IsNullOrWhiteSpace(databaseSchemaVersion)
            ? Value("VENNU_DATABASE_SCHEMA_VERSION", "local")
            : databaseSchemaVersion.Trim(),
        Value("VENNU_CONFIGURATION_SCHEMA_VERSION", "local"));

    private static string Value(string name, string fallback) =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)) ? fallback : Environment.GetEnvironmentVariable(name)!.Trim();

    private static int PositiveInteger(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) && value > 0 ? value : fallback;
}
