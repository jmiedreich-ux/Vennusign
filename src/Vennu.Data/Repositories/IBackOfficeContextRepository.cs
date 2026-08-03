namespace Vennu.Data.Repositories;

public sealed class BackOfficeContextRecord
{
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public Guid VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
}

public interface IBackOfficeContextRepository
{
    Task<IReadOnlyCollection<BackOfficeContextRecord>> GetAuthorizedAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
