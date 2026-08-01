using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class CloverPosProvider(ICloverCatalogGateway catalogGateway, ICloverInventoryGateway inventoryGateway) : IPosProvider
{
    public PosProvider Provider => PosProvider.Clover;

    public Task<PosCatalogResult> GetCatalogAsync(
        PosProviderContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return catalogGateway.GetCatalogAsync(context.ExternalMerchantId, context.AccessToken, cancellationToken);
    }

    public Task<PosInventoryResult> GetInventoryAsync(
        PosProviderContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return inventoryGateway.GetInventoryAsync(
            context.ExternalMerchantId,
            context.AccessToken,
            context.InventoryExternalItemIds ?? [],
            cancellationToken);
    }
}
