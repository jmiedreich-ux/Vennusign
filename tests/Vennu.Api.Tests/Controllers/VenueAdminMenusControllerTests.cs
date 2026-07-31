using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers.VenueAdmin;
using Vennu.Api.Services;
using Vennu.Api.Tests.E2E;
using Vennu.Api.VenueAdmin;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class VenueAdminMenusControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public VenueAdminMenusControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/venue-admin/menus");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_UsesAuthenticatedVenueClaim()
    {
        var venueId = Guid.NewGuid();
        var sections = new SectionServiceFake();
        var controller = new VenueAdminMenusController(sections, null!, null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(VenueAdminAuthenticationDefaults.VenueIdClaim, venueId.ToString())
                    ], "test"))
                }
            }
        };

        await controller.Get(CancellationToken.None);

        Assert.Equal(venueId, sections.RequestedVenueId);
    }

    private sealed class SectionServiceFake : IMenuSectionManagementService
    {
        public Guid? RequestedVenueId { get; private set; }

        public Task<MenuEditorSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default)
        {
            RequestedVenueId = venueId;
            return Task.FromResult(new MenuEditorSnapshot([], [], new MenuEditorCapabilities(false, false, false)));
        }

        public Task<MenuSection> CreateAsync(Guid venueId, Guid menuId, string name, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<MenuSection?> UpdateAsync(Guid venueId, Guid sectionId, string name, bool isActive, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<int> ReorderAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
