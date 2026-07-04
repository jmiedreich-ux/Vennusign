using System.Reflection;
using Microsoft.Extensions.Configuration;
using Vennu.DataAccess;

namespace Vennu.DataAccess.Tests;

public class SqlDataAccessTests
{
    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(1, 1)]
    [InlineData(42L, 42)]
    [InlineData((short)7, 7)]
    [InlineData((byte)3, 3)]
    [InlineData("ABC123", 1)]
    public void NormalizeInsertResult_ReturnsAffectedRowCountCompatibleValue(object insertResult, int expected)
    {
        var method = typeof(SqlDataAccess).GetMethod("NormalizeInsertResult", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        var actual = Assert.IsType<int>(method.Invoke(null, [insertResult]));
        Assert.Equal(expected, actual);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NormalizeInsertResult_ReturnsZero_ForNull()
    {
        var method = typeof(SqlDataAccess).GetMethod("NormalizeInsertResult", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        var actual = Assert.IsType<int>(method.Invoke(null, [null]));
        Assert.Equal(0, actual);
    }

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
