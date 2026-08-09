namespace Vennu.Api.Contracts.PlatformOperations;

public sealed record MenuCreateRequest(string Name);

public sealed record MenuSectionCreateRequest(string Name);

public sealed record MenuSectionUpdateRequest(string Name, bool IsActive);

public sealed record MenuSectionOrderRequest(IReadOnlyCollection<Guid>? SectionIds);

public sealed record MenuItemOrderRequest(IReadOnlyCollection<Guid>? ItemIds);

// Happy-hour pricing, quantities, tags and "popular" are owner-killed concepts
// (decision 6, Q14-r2); the write contract carries only what the library stores.
public sealed record MenuItemWriteRequest(
    string Name,
    string? Description,
    decimal Price);

public sealed record QuickAvailabilityRequest(bool IsAvailable);
