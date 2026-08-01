using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class PosCatalogMappingRepositoryTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 0, 50, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAllAsync_UsesVenueProviderScopeAndDeterministicOrder()
    {
        string? sql = null;
        object? parameters = null;
        var data = new FakeSqlDataAccess { ExecuteSqlQueryHandler = (value, args) => { sql = value; parameters = args; return []; } };
        var repository = new PosCatalogMappingRepository(data, new FixedTimeProvider(UtcNow));

        await repository.GetAllAsync(VenueId, PosProvider.Square);

        Assert.Contains("VenueId = @VenueId AND Provider = @Provider", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY EntityType, ExternalId, Id", sql, StringComparison.Ordinal);
        Assert.Equal(VenueId, Property<Guid>(parameters!, "VenueId"));
        Assert.Equal((int)PosProvider.Square, Property<int>(parameters!, "Provider"));
    }

    [Fact]
    public async Task SaveAsync_RejectsCrossVenueMapping()
    {
        var repository = new PosCatalogMappingRepository(new FakeSqlDataAccess(), new FixedTimeProvider(UtcNow));
        var mapping = Mapping();

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => repository.SaveAsync(Guid.NewGuid(), mapping));

        Assert.Equal("mapping", exception.ParamName);
    }

    [Fact]
    public async Task SaveAsync_NewMapping_AssignsIdentityAndTimestamps()
    {
        var data = new FakeSqlDataAccess();
        var repository = new PosCatalogMappingRepository(data, new FixedTimeProvider(UtcNow));

        var result = await repository.SaveAsync(VenueId, Mapping());

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(UtcNow.UtcDateTime, result.CreatedUtc);
        Assert.Equal(UtcNow.UtcDateTime, result.UpdatedUtc);
        Assert.Same(result, Assert.Single(data.InsertedEntities));
    }

    private static PosCatalogMapping Mapping() => new()
    {
        VenueId = VenueId,
        Provider = PosProvider.Square,
        EntityType = PosCatalogEntityType.Item,
        ExternalId = "variation-1",
        LocalEntityId = Guid.NewGuid()
    };

    private static T Property<T>(object value, string name) => (T)value.GetType().GetProperty(name)!.GetValue(value)!;

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
