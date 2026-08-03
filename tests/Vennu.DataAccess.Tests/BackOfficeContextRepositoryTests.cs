using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class BackOfficeContextRepositoryTests
{
    [Fact]
    public async Task GetAuthorizedAsync_UsesActiveMembershipsAndManageContentRoles()
    {
        string? sql = null;
        object? parameters = null;
        var userId = Guid.NewGuid();
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, capturedParameters) =>
            {
                sql = capturedSql;
                parameters = capturedParameters;
                return [];
            }
        };

        var result = await new BackOfficeContextRepository(data).GetAuthorizedAsync(userId);

        Assert.Empty(result);
        Assert.Contains("om.RevokedUtc IS NULL", sql, StringComparison.Ordinal);
        Assert.Contains("om.Role IN (1, 2)", sql, StringComparison.Ordinal);
        Assert.Contains("vm.RevokedUtc IS NULL AND vm.Role IN (1, 2)", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY o.Name, v.Name, v.Id", sql, StringComparison.Ordinal);
        Assert.Equal(userId, parameters!.GetType().GetProperty("UserId")!.GetValue(parameters));
    }

    [Fact]
    public async Task GetAuthorizedAsync_RejectsEmptyUserId()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new BackOfficeContextRepository(new FakeSqlDataAccess()).GetAuthorizedAsync(Guid.Empty));
    }
}
