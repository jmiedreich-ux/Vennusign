using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers.BackOffice;
using Vennu.Api.Services;
using Vennu.Api.Tests.E2E;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeMenusControllerTests : IClassFixture<VennuApiFactory>
{
    private readonly VennuApiFactory factory;

    public BackOfficeMenusControllerTests(VennuApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenVenueTokenIsMissing()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/back-office/menus");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_UsesAuthenticatedVenueClaim()
    {
        var venueId = Guid.NewGuid();
        var sections = new SectionServiceFake();
        var controller = new BackOfficeMenusController(sections, null!, null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(BackOfficeAuthenticationDefaults.VenueIdClaim, venueId.ToString())
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

        public Task<Menu> CreateMenuAsync(Guid venueId, string name, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<MenuSection> CreateAsync(Guid venueId, Guid menuId, string name, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<MenuSection?> UpdateAsync(Guid venueId, Guid sectionId, string name, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<int> ReorderAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
