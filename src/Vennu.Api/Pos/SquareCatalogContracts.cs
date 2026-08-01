using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public interface ISquareCatalogGateway
{
    Task<PosCatalogResult> GetCatalogAsync(string accessToken, CancellationToken cancellationToken = default);
}
