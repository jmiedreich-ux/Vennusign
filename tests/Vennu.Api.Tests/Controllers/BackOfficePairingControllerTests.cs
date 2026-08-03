using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers.BackOffice;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Api.BackOffice;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficePairingControllerTests
{
    [Fact]
    public async Task PreviewReplacement_ReturnsImpactWithoutMutation()
    {
        var venueId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var service = new FakeReplacementService
        {
            PreviewResult = new(ScreenReplacementStatus.Ready, targetId, Guid.NewGuid(), "Lobby", "tizen", "2.0.0", "Main", 2, true, true, true)
        };
        var controller = CreateController(venueId, service);

        var result = await controller.PreviewReplacement(new(targetId, "123456", false), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<Vennu.Api.Contracts.PlatformOperations.ScreenReplacementResponse>(ok.Value);
        Assert.Equal("Ready", response.Status);
        Assert.True(response.PreservesConfiguration);
        Assert.True(response.PreservesHistory);
        Assert.True(response.PreservesVideoWall);
        Assert.False(service.ReplaceCalled);
    }

    [Fact]
    public async Task Replace_RequiresExplicitConfirmation()
    {
        var controller = CreateController(Guid.NewGuid(), new FakeReplacementService());
        var result = await controller.Replace(new(Guid.NewGuid(), "123456", false), CancellationToken.None);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Theory]
    [InlineData(ScreenReplacementStatus.PairingCodeExpired, StatusCodes.Status410Gone)]
    [InlineData(ScreenReplacementStatus.PairingCodeClaimed, StatusCodes.Status409Conflict)]
    [InlineData(ScreenReplacementStatus.SourceAlreadyAssigned, StatusCodes.Status409Conflict)]
    [InlineData(ScreenReplacementStatus.TargetNotFound, StatusCodes.Status404NotFound)]
    public async Task Replace_MapsRecoverableFailureStates(ScreenReplacementStatus status, int expectedStatus)
    {
        var service = new FakeReplacementService { ReplaceResult = new(status) };
        var controller = CreateController(Guid.NewGuid(), service);
        var result = await controller.Replace(new(Guid.NewGuid(), "123456", true, DateTime.UtcNow), CancellationToken.None);
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.Equal(expectedStatus, objectResult.StatusCode);
    }

    [Fact]
    public async Task Replace_ReturnsPriorCompletionForIdempotentRetry()
    {
        var completedUtc = DateTime.UtcNow;
        var service = new FakeReplacementService { ReplaceResult = new(ScreenReplacementStatus.Completed, Guid.NewGuid(), Guid.NewGuid(), CompletedUtc: completedUtc) };
        var controller = CreateController(Guid.NewGuid(), service);
        var result = await controller.Replace(new(Guid.NewGuid(), "123456", true, DateTime.UtcNow), CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<Vennu.Api.Contracts.PlatformOperations.ScreenReplacementResponse>(ok.Value);
        Assert.Equal(completedUtc, response.CompletedUtc);
    }

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
        var controller = new BackOfficePairingController(
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
                        [new Claim(BackOfficeAuthenticationDefaults.VenueIdClaim, venueId.ToString())],
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

    private static BackOfficePairingController CreateController(Guid venueId, IScreenReplacementService replacement) =>
        new(new FakeScreenRepository(), new FakeScreenPairingCodeRepository(), new FakeVenueRepository(), new AllowingEntitlement(), replacement)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(BackOfficeAuthenticationDefaults.VenueIdClaim, venueId.ToString()), new Claim(ClaimTypes.Name, "operator@example.com")],
                        "test"))
                }
            }
        };

    private sealed class AllowingEntitlement : IVenueEntitlementService
    {
        public Task EnsureCanAddScreenAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task EnsureCanAddVenueAsync(Guid organizationId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeReplacementService : IScreenReplacementService
    {
        public ScreenReplacementResult PreviewResult { get; init; } = new(ScreenReplacementStatus.Ready);
        public ScreenReplacementResult ReplaceResult { get; init; } = new(ScreenReplacementStatus.Completed);
        public bool ReplaceCalled { get; private set; }
        public Task<ScreenReplacementResult> PreviewAsync(Guid venueId, Guid targetScreenId, string pairingCode, CancellationToken cancellationToken = default) => Task.FromResult(PreviewResult);
        public Task<ScreenReplacementResult> ReplaceAsync(Guid venueId, Guid targetScreenId, string pairingCode, DateTime expectedTargetUpdatedUtc, string actor, CancellationToken cancellationToken = default)
        {
            ReplaceCalled = true;
            return Task.FromResult(ReplaceResult);
        }
    }
}
