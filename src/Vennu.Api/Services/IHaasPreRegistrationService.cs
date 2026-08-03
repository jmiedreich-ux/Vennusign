using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Contracts.Screens;

namespace Vennu.Api.Services;

public interface IHaasPreRegistrationService
{
    Task<HaasPreRegistrationResponse> CreateAsync(
        Guid venueId,
        HaasPreRegistrationRequest request,
        CancellationToken cancellationToken = default);

    Task<ClaimPreRegisteredScreenResponse?> ClaimAsync(
        ClaimPreRegisteredScreenRequest request,
        CancellationToken cancellationToken = default);
}
