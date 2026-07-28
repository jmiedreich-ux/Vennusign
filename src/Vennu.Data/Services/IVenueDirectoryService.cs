namespace Vennu.Data.Services;

public interface IVenueDirectoryService
{
    Task<IReadOnlyCollection<VenueDirectoryItem>> SearchAsync(
        VenueDirectoryQuery query,
        CancellationToken cancellationToken = default);
}

