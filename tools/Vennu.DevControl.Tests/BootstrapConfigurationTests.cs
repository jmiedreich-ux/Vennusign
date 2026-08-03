namespace Vennu.DevControl.Tests;

public sealed class BootstrapConfigurationTests
{
    [Fact]
    public void EnvironmentProvider_RequiresA256BitKeyAndAppliesOnlyRelevantValues()
    {
        var localKey = BootstrapConfiguration.GenerateLocalKey();

        var valid = BootstrapConfiguration.TryCreate(
            "Development",
            "Server=(localdb)\\MSSQLLocalDB;Database=VennuSign;",
            "Environment",
            localKey,
            "https://vault.example/keys/ignored",
            out var configuration,
            out var error);
        var environment = new Dictionary<string, string?>
        {
            [BootstrapConfiguration.KeyIdVariable] = "stale"
        };
        configuration!.ApplyTo(environment);

        Assert.True(valid);
        Assert.Null(error);
        Assert.Equal(localKey, environment[BootstrapConfiguration.LocalKeyVariable]);
        Assert.False(environment.ContainsKey(BootstrapConfiguration.KeyIdVariable));
    }

    [Fact]
    public void AzureProvider_RequiresHttpsAndRemovesLocalKey()
    {
        Assert.False(BootstrapConfiguration.TryCreate(
            "Production", "Server=sql;Database=Vennu;", "AzureKeyVault", null, "http://vault.example/key", out _, out _));

        Assert.True(BootstrapConfiguration.TryCreate(
            "Production", "Server=sql;Database=Vennu;", "AzureKeyVault", null, "https://vault.example/keys/configuration", out var configuration, out _));
        var environment = new Dictionary<string, string?> { [BootstrapConfiguration.LocalKeyVariable] = "stale" };
        configuration!.ApplyTo(environment);

        Assert.Equal("https://vault.example/keys/configuration", environment[BootstrapConfiguration.KeyIdVariable]);
        Assert.False(environment.ContainsKey(BootstrapConfiguration.LocalKeyVariable));
    }

    [Theory]
    [InlineData(null, "connection", "Environment", "invalid")]
    [InlineData("Unknown", "connection", "Environment", "invalid")]
    [InlineData("Development", "", "Environment", "invalid")]
    [InlineData("Development", "connection", "Unknown", "invalid")]
    public void InvalidCombinationsAreRejected(string? environment, string connection, string provider, string localKey)
    {
        Assert.False(BootstrapConfiguration.TryCreate(environment, connection, provider, localKey, null, out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
