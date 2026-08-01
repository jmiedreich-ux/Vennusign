namespace Vennu.Data.Services;

public interface IPosCatalogImportService
{
    Task<PosCatalogImportResult> ImportAsync(Guid venueId, CancellationToken cancellationToken = default);
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
