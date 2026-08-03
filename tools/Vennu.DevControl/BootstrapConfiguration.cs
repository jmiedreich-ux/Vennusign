using System.Security.Cryptography;

namespace Vennu.DevControl;

public sealed class BootstrapConfiguration
{
    public const string EnvironmentVariable = "VENU_CONFIGURATION_ENVIRONMENT";
    public const string ConnectionStringVariable = "VENU_CONFIGURATION_CONNECTION_STRING";
    public const string KeyProviderVariable = "VENU_CONFIGURATION_KEY_PROVIDER";
    public const string LocalKeyVariable = "VENU_CONFIGURATION_LOCAL_KEY";
    public const string KeyIdVariable = "VENU_CONFIGURATION_KEY_ID";

    private static readonly HashSet<string> Environments = ["Development", "Test", "Staging", "Production"];
    private static readonly HashSet<string> Providers = ["Environment", "AzureKeyVault"];

    private BootstrapConfiguration(IReadOnlyDictionary<string, string> values)
    {
        Values = values;
    }

    public IReadOnlyDictionary<string, string> Values { get; }

    public static bool TryCreate(
        string? environmentName,
        string? connectionString,
        string? keyProvider,
        string? localKey,
        string? keyId,
        out BootstrapConfiguration? configuration,
        out string? error)
    {
        configuration = null;
        if (!Environments.Contains(environmentName ?? string.Empty))
        {
            error = "Select a supported configuration environment.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            error = "Enter the configuration database connection string.";
            return false;
        }
        if (!Providers.Contains(keyProvider ?? string.Empty))
        {
            error = "Select a supported key provider.";
            return false;
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [EnvironmentVariable] = environmentName!,
            [ConnectionStringVariable] = connectionString.Trim(),
            [KeyProviderVariable] = keyProvider!
        };
        if (keyProvider == "Environment")
        {
            if (!IsValidLocalKey(localKey))
            {
                error = "The local key must be a Base64-encoded 256-bit key.";
                return false;
            }
            values[LocalKeyVariable] = localKey!;
        }
        else
        {
            if (!Uri.TryCreate(keyId, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            {
                error = "The Azure Key Vault key ID must be an absolute HTTPS URI.";
                return false;
            }
            values[KeyIdVariable] = uri.AbsoluteUri;
        }

        configuration = new BootstrapConfiguration(values);
        error = null;
        return true;
    }

    public static string GenerateLocalKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public void ApplyTo(IDictionary<string, string?> environment)
    {
        foreach (var name in VariableNames) environment.Remove(name);
        foreach (var value in Values) environment[value.Key] = value.Value;
    }

    public static IReadOnlyList<string> VariableNames { get; } =
    [
        EnvironmentVariable,
        ConnectionStringVariable,
        KeyProviderVariable,
        LocalKeyVariable,
        KeyIdVariable
    ];

    private static bool IsValidLocalKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        try { return Convert.FromBase64String(value).Length == 32; }
        catch (FormatException) { return false; }
    }
}
