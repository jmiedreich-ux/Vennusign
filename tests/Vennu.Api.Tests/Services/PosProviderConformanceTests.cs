using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class PosProviderConformanceTests
{
    [Fact]
    public async Task AllProviders_ExposeUniqueIdentityAndForwardCatalogCancellation()
    {
        using var source = new CancellationTokenSource();
        var squareGateway = new SquareCatalogFake();
        var toastCatalog = new ToastCatalogFake();
        var cloverCatalog = new CloverCatalogFake();
        IPosProvider[] providers =
        [
            new SquarePosProvider(squareGateway),
            new ToastPosProvider(toastCatalog, new ToastInventoryFake()),
            new CloverPosProvider(cloverCatalog, new CloverInventoryFake())
        ];
        var context = new PosProviderContext(Guid.NewGuid(), "merchant-1", "access-secret", ["item-1"]);

        foreach (var provider in providers)
            await provider.GetCatalogAsync(context, source.Token);

        Assert.Equal([PosProvider.Square, PosProvider.Toast, PosProvider.Clover], providers.Select(value => value.Provider));
        Assert.Equal(source.Token, squareGateway.Token);
        Assert.Equal(source.Token, toastCatalog.Token);
        Assert.Equal(source.Token, cloverCatalog.Token);
        Assert.Equal("merchant-1", toastCatalog.MerchantId);
        Assert.Equal("merchant-1", cloverCatalog.MerchantId);
    }

    [Fact]
    public async Task InventorySupport_IsExplicitAndProviderScoped()
    {
        var toastInventory = new ToastInventoryFake();
        var cloverInventory = new CloverInventoryFake();
        var context = new PosProviderContext(Guid.NewGuid(), "merchant-1", "access-secret", ["item-1"]);
        var square = new SquarePosProvider(new SquareCatalogFake());
        var toast = new ToastPosProvider(new ToastCatalogFake(), toastInventory);
        var clover = new CloverPosProvider(new CloverCatalogFake(), cloverInventory);

        await Assert.ThrowsAsync<NotSupportedException>(() => square.GetInventoryAsync(context));
        await toast.GetInventoryAsync(context);
        await clover.GetInventoryAsync(context);

        Assert.Equal(["item-1"], toastInventory.ItemIds);
        Assert.Equal(["item-1"], cloverInventory.ItemIds);
        Assert.Equal("merchant-1", cloverInventory.MerchantId);
    }

    private sealed class SquareCatalogFake : ISquareCatalogGateway
    {
        public CancellationToken Token { get; private set; }
        public Task<PosCatalogResult> GetCatalogAsync(string accessToken, CancellationToken cancellationToken = default)
        { Token = cancellationToken; return Task.FromResult(new PosCatalogResult([], [])); }
    }

    private sealed class ToastCatalogFake : IToastCatalogGateway
    {
        public string? MerchantId { get; private set; }
        public CancellationToken Token { get; private set; }
        public Task<PosCatalogResult> GetCatalogAsync(string restaurantGuid, string accessToken, CancellationToken cancellationToken = default)
        { MerchantId = restaurantGuid; Token = cancellationToken; return Task.FromResult(new PosCatalogResult([], [])); }
    }

    private sealed class ToastInventoryFake : IToastInventoryGateway
    {
        public IReadOnlyCollection<string>? ItemIds { get; private set; }
        public Task<PosInventoryResult> GetInventoryAsync(string restaurantGuid, string accessToken, IReadOnlyCollection<string> externalItemIds, CancellationToken cancellationToken = default)
        { ItemIds = externalItemIds; return Task.FromResult(new PosInventoryResult([], DateTimeOffset.UtcNow)); }
    }

    private sealed class CloverCatalogFake : ICloverCatalogGateway
    {
        public string? MerchantId { get; private set; }
        public CancellationToken Token { get; private set; }
        public Task<PosCatalogResult> GetCatalogAsync(string merchantId, string accessToken, CancellationToken cancellationToken = default)
        { MerchantId = merchantId; Token = cancellationToken; return Task.FromResult(new PosCatalogResult([], [])); }
    }

    private sealed class CloverInventoryFake : ICloverInventoryGateway
    {
        public string? MerchantId { get; private set; }
        public IReadOnlyCollection<string>? ItemIds { get; private set; }
        public Task<PosInventoryResult> GetInventoryAsync(string merchantId, string accessToken, IReadOnlyCollection<string> externalItemIds, CancellationToken cancellationToken = default)
        { MerchantId = merchantId; ItemIds = externalItemIds; return Task.FromResult(new PosInventoryResult([], DateTimeOffset.UtcNow)); }
    }
}
