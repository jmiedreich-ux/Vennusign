using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.Venues;
using Vennu.Api.Controllers.PlatformOperations;
using Vennu.Api.Tests.E2E;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class PlatformOperationsVenueProvisioningControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public PlatformOperationsVenueProvisioningControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenAdminKeyIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/platform-operations/venues", ValidRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreatedResponse_FromProvisioningService()
    {
        var venueId = Guid.NewGuid();
        var controller = CreateController(new ProvisioningServiceFake
        {
            Result = new VenueProvisioningResult(
                venueId,
                new VenueSubscription { VenueId = venueId, TierId = Guid.NewGuid(), Status = "trialing" })
        });

        var action = await controller.Create(ValidRequest(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(action.Result);
        var response = Assert.IsType<CreateVenueResponse>(created.Value);
        Assert.Equal(venueId, response.VenueId);
        Assert.Equal(nameof(PlatformOperationsVenuesController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenCommercialStateCannotBeInitialized()
    {
        var controller = CreateController(new ProvisioningServiceFake
        {
            Exception = new InvalidOperationException("The Starter subscription tier is unavailable.")
        });

        var action = await controller.Create(ValidRequest(), CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(action.Result);
        var problem = Assert.IsType<ProblemDetails>(conflict.Value);
        Assert.Equal("Venue provisioning failed", problem.Title);
    }

    private static PlatformOperationsVenuesController CreateController(IVenueProvisioningService provisioningService) =>
        new(null!, null!, null!, null!, provisioningService);

    private static CreateVenueRequest ValidRequest() => new()
    {
        Name = "Harbor Café",
        Timezone = "America/New_York",
        Type = "Café",
        PrimaryLanguage = "en"
    };

    private sealed class ProvisioningServiceFake : IVenueProvisioningService
    {
        public VenueProvisioningResult? Result { get; init; }
        public Exception? Exception { get; init; }

        public Task<VenueProvisioningResult> ProvisionAsync(
            Venue venue,
            CancellationToken cancellationToken = default)
        {
            return Exception is null
                ? Task.FromResult(Result!)
                : Task.FromException<VenueProvisioningResult>(Exception);
        }
    }
}
