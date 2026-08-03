using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers.VenueAdmin;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Api.VenueAdmin;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class VenueAdminPairingControllerTests
{
    [Fact]
    public async Task Claim_ReturnsConflictBeforePairingMutation_WhenScreenLimitIsReached()
    {
        var venueId = Guid.NewGuid();
        var pairingLookupAttempted = false;
        var pairingCodes = new FakeScreenPairingCodeRepository
        {
            GetByCodeAsyncHandler = (_, _) =>
            {
                pairingLookupAttempted = true;
                return Task.FromResult<Vennu.Core.Models.ScreenPairingCode?>(null);
            }
        };
        var controller = new VenueAdminPairingController(
            new FakeScreenRepository(),
            pairingCodes,
            new FakeVenueRepository(),
            new RejectingEntitlement())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(VenueAdminAuthenticationDefaults.VenueIdClaim, venueId.ToString())],
                        "test"))
                }
            }
        };

        var result = await controller.Claim("123456", CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        var details = Assert.IsType<ProblemDetails>(conflict.Value);
        Assert.Equal(StatusCodes.Status409Conflict, details.Status);
        Assert.Equal("Screen limit reached.", details.Title);
        Assert.False(pairingLookupAttempted);
    }

    private sealed class RejectingEntitlement : IVenueEntitlementService
    {
        public Task EnsureCanAddScreenAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromException(new TierScreenLimitReachedException());

        public Task EnsureCanAddVenueAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
