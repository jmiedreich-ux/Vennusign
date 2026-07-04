using Microsoft.Extensions.Configuration;
using Vennu.DataAccess;

namespace Vennu.DataAccess.Tests;

public class SqlDataAccessTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void GetFieldList_ReturnsParsedFields()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionString"] = "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
            })
            .Build();

        var sut = new SqlDataAccess(configuration);

        var fields = sut.GetFieldList("Id|Name");

        Assert.NotNull(fields);
        Assert.Collection(
            fields!,
            field => Assert.Equal("Id", field.Name),
            field => Assert.Equal("Name", field.Name));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetFieldList_ReturnsNull_ForEmptyInput()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionString"] = "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"
            })
            .Build();

        var sut = new SqlDataAccess(configuration);

        Assert.Null(sut.GetFieldList("  "));
    }
}
