namespace Vennu.Data.Services;

using Vennu.Core.Models;

public interface IPosCatalogImportService
{
    Task<PosCatalogImportResult> ImportAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<PosCatalogImportResult> ImportAsync(
        Guid venueId,
        PosProvider provider,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}

public sealed record PosCatalogImportResult(
    string Status,
    int CategoriesCreated,
    int CategoriesUpdated,
    int ItemsCreated,
    int ItemsUpdated,
    int ModifiersMapped,
    IReadOnlyCollection<string> Conflicts,
    DateTime CompletedUtc);
