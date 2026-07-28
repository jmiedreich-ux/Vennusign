namespace Vennu.Data.Services;

public interface IVenueSupportDetailService
{
    Task<VenueSupportDetail?> GetAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);
}
