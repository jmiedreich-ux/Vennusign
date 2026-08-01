using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class CustomerAuthenticationRepositoryTests
{
    private const string Hash = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public async Task GetSessionByTokenHashAsync_UsesHashOnly()
    {
        string? sql = null;
        object? parameters = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, capturedParameters) =>
            {
                sql = capturedSql;
                parameters = capturedParameters;
                return [];
            }
        };

        Assert.Null(await new CustomerAuthenticationRepository(data).GetSessionByTokenHashAsync(Hash.ToLowerInvariant()));

        Assert.Contains("WHERE TokenHash = @TokenHash", sql, StringComparison.Ordinal);
        Assert.Equal(Hash, Property<string>(parameters!, "TokenHash"));
    }

    [Fact]
    public async Task ConsumeEmailLoginTokenAsync_IsAtomicSingleUseAndExpiryBounded()
    {
        string? sql = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (capturedSql, _) =>
            {
                sql = capturedSql;
                return [];
            }
        };

        Assert.Null(await new CustomerAuthenticationRepository(data)
            .ConsumeEmailLoginTokenAsync(Hash, DateTime.UtcNow));

        Assert.Contains("WITH (UPDLOCK, ROWLOCK)", sql, StringComparison.Ordinal);
        Assert.Contains("ConsumedUtc IS NULL", sql, StringComparison.Ordinal);
        Assert.Contains("ExpiresUtc > @ConsumedUtc", sql, StringComparison.Ordinal);
        Assert.Contains("OUTPUT inserted.Id", sql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateEmailLoginTokenAsync_RejectsExternalReturnPath()
    {
        var token = new EmailLoginToken { TokenHash = Hash, ReturnPath = "https://attacker.example" };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new CustomerAuthenticationRepository(new FakeSqlDataAccess()).CreateEmailLoginTokenAsync(token));
    }

    private static T Property<T>(object value, string name) =>
        (T)value.GetType().GetProperty(name)!.GetValue(value)!;
}
