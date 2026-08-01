using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class SquarePosProvider(ISquareCatalogGateway gateway) : IPosProvider
{
    public PosProvider Provider => PosProvider.Square;

    public Task<PosCatalogResult> GetCatalogAsync(PosProviderContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return gateway.GetCatalogAsync(context.AccessToken, cancellationToken);
    }

    public Task<PosInventoryResult> GetInventoryAsync(PosProviderContext context, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Square inventory synchronization begins in WP-12.05.");
}
