using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class CloverPosProvider(ICloverCatalogGateway gateway) : IPosProvider
{
    public PosProvider Provider => PosProvider.Clover;

    public Task<PosCatalogResult> GetCatalogAsync(
        PosProviderContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return gateway.GetCatalogAsync(context.ExternalMerchantId, context.AccessToken, cancellationToken);
    }

    public Task<PosInventoryResult> GetInventoryAsync(
        PosProviderContext context,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Clover inventory synchronization begins in WP-12.09.");
}
