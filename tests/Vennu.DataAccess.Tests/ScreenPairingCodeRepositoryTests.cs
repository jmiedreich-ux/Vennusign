using Vennu.Data.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public class ScreenPairingCodeRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task ClaimAsync_ReturnsFalse_WhenCodeIsExpired()
    {
        var pairingCode = new ScreenPairingCode
        {
            Code = "ABC123",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        var dataAccess = new FakeSqlDataAccess
        {
            QueryHandler = _ => pairingCode
        };

        var sut = new ScreenPairingCodeRepository(dataAccess);

        var claimed = await sut.ClaimAsync(pairingCode.Code, Guid.NewGuid());

        Assert.False(claimed);
        Assert.Empty(dataAccess.UpdatedEntities);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ClaimAsync_ClaimsActiveCode()
    {
        var pairingCode = new ScreenPairingCode
        {
            Code = "XYZ789",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsClaimed = false
        };

        var venueId = Guid.NewGuid();
        var dataAccess = new FakeSqlDataAccess
        {
            QueryHandler = _ => pairingCode,
            UpdateResult = 1
        };

        var sut = new ScreenPairingCodeRepository(dataAccess);
        using var cancellationSource = new CancellationTokenSource();

        var claimed = await sut.ClaimAsync(pairingCode.Code, venueId, cancellationSource.Token);

        Assert.True(claimed);
        Assert.True(pairingCode.IsClaimed);
        Assert.Equal(venueId, pairingCode.VenueId);
        Assert.NotNull(pairingCode.ClaimedAt);
        Assert.Single(dataAccess.UpdatedEntities);
        Assert.Equal(cancellationSource.Token, dataAccess.LastCancellationToken);
    }
}
