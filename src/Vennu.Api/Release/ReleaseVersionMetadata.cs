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
    public static ReleaseVersionMetadata FromEnvironment() => new(
        Value("VENNU_PRODUCT_VERSION", "0.0.0-local"),
        Value("VENNU_COMPONENT_VERSION", "0.0.0-local"),
        PositiveInteger("VENNU_API_CONTRACT_MAJOR", 1),
        Value("VENNU_SOURCE_COMMIT", "local"),
        Value("VENNU_BUILD_ID", "local"),
        Value("VENNU_DATABASE_SCHEMA_VERSION", "local"),
        Value("VENNU_CONFIGURATION_SCHEMA_VERSION", "local"));

    private static string Value(string name, string fallback) =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)) ? fallback : Environment.GetEnvironmentVariable(name)!.Trim();

    private static int PositiveInteger(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) && value > 0 ? value : fallback;
}
