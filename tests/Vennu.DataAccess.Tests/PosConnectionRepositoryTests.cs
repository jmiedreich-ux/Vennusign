using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class PosConnectionRepositoryTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 31, 23, 20, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAsync_UsesVenueAndProviderScope()
    {
        string? capturedSql = null;
        object? capturedParameters = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, parameters) =>
            {
                capturedSql = sql;
                capturedParameters = parameters;
                return [];
            }
        };
        var repository = new PosConnectionRepository(dataAccess, new FixedTimeProvider(UtcNow));

        await repository.GetAsync(VenueId, PosProvider.Square);

        Assert.Contains("VenueId = @VenueId AND Provider = @Provider", capturedSql, StringComparison.Ordinal);
        Assert.Equal(VenueId, Property<Guid>(capturedParameters!, "VenueId"));
        Assert.Equal((int)PosProvider.Square, Property<int>(capturedParameters!, "Provider"));
    }

    [Fact]
    public async Task GetAllByVenueIdAsync_UsesDeterministicProviderOrder()
    {
        string? capturedSql = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, _) =>
            {
                capturedSql = sql;
                return [];
            }
        };
        var repository = new PosConnectionRepository(dataAccess, new FixedTimeProvider(UtcNow));

        await repository.GetAllByVenueIdAsync(VenueId);

        Assert.Contains("WHERE VenueId = @VenueId", capturedSql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY Provider, Id", capturedSql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetByExternalMerchantIdAsync_UsesProviderMerchantScope()
    {
        string? capturedSql = null;
        object? capturedParameters = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, parameters) =>
            {
                capturedSql = sql;
                capturedParameters = parameters;
                return [];
            }
        };
        var repository = new PosConnectionRepository(dataAccess, new FixedTimeProvider(UtcNow));

        Assert.Null(await repository.GetByExternalMerchantIdAsync(PosProvider.Square, " merchant-1 "));

        Assert.Contains("Provider = @Provider AND ExternalMerchantId = @ExternalMerchantId", capturedSql, StringComparison.Ordinal);
        Assert.Equal((int)PosProvider.Square, Property<int>(capturedParameters!, "Provider"));
        Assert.Equal("merchant-1", Property<string>(capturedParameters!, "ExternalMerchantId"));
    }

    [Fact]
    public async Task SaveAsync_NewConnection_AssignsIdentityAndTimestamps()
    {
        var dataAccess = new FakeSqlDataAccess();
        var repository = new PosConnectionRepository(dataAccess, new FixedTimeProvider(UtcNow));
        var connection = Connection();

        var saved = await repository.SaveAsync(VenueId, connection);

        Assert.NotEqual(Guid.Empty, saved.Id);
        Assert.Equal(UtcNow.UtcDateTime, saved.CreatedUtc);
        Assert.Equal(UtcNow.UtcDateTime, saved.UpdatedUtc);
        Assert.Same(saved, Assert.Single(dataAccess.InsertedEntities));
    }

    [Fact]
    public async Task SaveAsync_ExistingConnection_PreservesVenueProviderAndIdentity()
    {
        var existing = Connection();
        existing.Id = Guid.NewGuid();
        existing.ExternalMerchantId = "old-merchant";
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (_, _) => [existing]
        };
        var repository = new PosConnectionRepository(dataAccess, new FixedTimeProvider(UtcNow));
        var replacement = Connection();
        replacement.ExternalMerchantId = "new-merchant";

        var saved = await repository.SaveAsync(VenueId, replacement);

        Assert.Same(existing, saved);
        Assert.Equal("new-merchant", existing.ExternalMerchantId);
        Assert.Equal(VenueId, existing.VenueId);
        Assert.Equal(PosProvider.Square, existing.Provider);
        Assert.Same(existing, Assert.Single(dataAccess.UpdatedEntities));
    }

    [Fact]
    public async Task SaveAsync_RejectsCrossVenueConnection()
    {
        var repository = new PosConnectionRepository(new FakeSqlDataAccess(), new FixedTimeProvider(UtcNow));
        var connection = Connection();

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.SaveAsync(Guid.NewGuid(), connection));

        Assert.Equal("connection", exception.ParamName);
    }

    [Fact]
    public async Task DeleteAsync_UsesVenueAndProviderScope()
    {
        string? capturedSql = null;
        object? capturedParameters = null;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, parameters) =>
            {
                capturedSql = sql;
                capturedParameters = parameters;
                return [];
            }
        };
        var repository = new PosConnectionRepository(dataAccess, new FixedTimeProvider(UtcNow));

        Assert.False(await repository.DeleteAsync(VenueId, PosProvider.Square));
        Assert.Contains("VenueId = @VenueId AND Provider = @Provider", capturedSql, StringComparison.Ordinal);
        Assert.Equal(VenueId, Property<Guid>(capturedParameters!, "VenueId"));
        Assert.Equal((int)PosProvider.Square, Property<int>(capturedParameters!, "Provider"));
    }

    private static PosConnection Connection() => new()
    {
        VenueId = VenueId,
        Provider = PosProvider.Square,
        Status = PosConnectionStatus.Connected,
        ExternalMerchantId = "merchant-1",
        ProtectedAccessToken = "protected:access"
    };

    private static T Property<T>(object value, string name) =>
        (T)value.GetType().GetProperty(name)!.GetValue(value)!;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
