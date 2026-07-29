namespace Vennu.Api.Contracts.Admin;

public sealed record MenuSectionCreateRequest(string Name);

public sealed record MenuSectionUpdateRequest(string Name, bool IsActive);

public sealed record MenuSectionOrderRequest(IReadOnlyCollection<Guid>? SectionIds);

public sealed record MenuItemWriteRequest(
    string Name,
    string? Description,
    decimal Price,
    decimal? HappyHourPrice);
