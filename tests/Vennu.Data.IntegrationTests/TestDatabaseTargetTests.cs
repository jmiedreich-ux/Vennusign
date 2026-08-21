using Microsoft.Data.SqlClient;
using Vennu.Data.IntegrationTests.Fixtures;

namespace Vennu.Data.IntegrationTests;

/// <summary>
/// How a run decides which database it talks to. These are unit tests on purpose:
/// the point is the decision, and the decision must be checkable without an Azure
/// sign-in or a database.
/// </summary>
[Trait("Category", "Unit")]
public class TestDatabaseTargetTests
{
    [Fact]
    public void TheDefaultTargetIsLocalDbWithNoUserAndNoPassword()
    {
        Assert.False(TestDatabaseTarget.WantsAzure(null));
        Assert.False(TestDatabaseTarget.WantsAzure(""));
        Assert.False(TestDatabaseTarget.WantsAzure("localdb"));

        var builder = new SqlConnectionStringBuilder(TestDatabaseTarget.LocalDbConnectionString);
        Assert.True(builder.IntegratedSecurity);
        Assert.Empty(builder.UserID);
        Assert.Empty(builder.Password);
        Assert.Contains("localdb", builder.DataSource, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("azure")]
    [InlineData("Azure")]
    [InlineData("  AZURE  ")]
    public void AzureIsOptedIntoExplicitly(string target) =>
        Assert.True(TestDatabaseTarget.WantsAzure(target));

    [Fact]
    public void TheVaultDefaultsToTheDevVaultAndCanBePointedElsewhere()
    {
        Assert.Equal("kv-vennusign-dev", TestDatabaseTarget.VaultName(null));
        Assert.Equal("kv-vennusign-dev", TestDatabaseTarget.VaultName("  "));
        Assert.Equal("kv-vennusign-other", TestDatabaseTarget.VaultName(" kv-vennusign-other "));
    }

    [Fact]
    public void NothingIsSaidWhenTheRetiredVariableIsAbsent()
    {
        Assert.Null(TestDatabaseTarget.RetiredVariableWarning(null, null));
        Assert.Null(TestDatabaseTarget.RetiredVariableWarning("", "azure"));
        Assert.Null(TestDatabaseTarget.RetiredVariableWarning("   ", null));
    }

    [Fact]
    public void TheRetiredVariableIsIgnoredLoudlyAndTheRunStaysOnLocalDb()
    {
        // The exact value that produced 115 failures reading as product breakage:
        // a whole connection string with a stale sqladmin password, aimed at the
        // dev product database, inherited rather than set by anyone present.
        var stale = "Server=tcp:dev-vennusign.database.windows.net,1433;Initial Catalog=dev_vennusign;"
                  + "User ID=sqladmin;Password=whatever-it-used-to-be;Encrypt=True;";

        var warning = TestDatabaseTarget.RetiredVariableWarning(stale, null);

        Assert.NotNull(warning);
        Assert.Contains("no longer supported", warning);
        Assert.Contains("being ignored", warning);
        Assert.Contains("LocalDB", warning);
        // The part that saves the afternoon: why it is set when nobody set it.
        Assert.Contains("wsl --shutdown", warning);
        // And it must not echo the credential it is complaining about.
        Assert.DoesNotContain("whatever-it-used-to-be", warning);
        Assert.DoesNotContain("Password=", warning);
    }

    [Fact]
    public void TheRetiredVariableIsAlsoIgnoredWhenAzureIsAskedFor()
    {
        var warning = TestDatabaseTarget.RetiredVariableWarning("Server=tcp:x;Password=y;", "azure");

        Assert.NotNull(warning);
        Assert.Contains("Key Vault", warning);
        Assert.DoesNotContain("Password=y", warning);
    }
}
