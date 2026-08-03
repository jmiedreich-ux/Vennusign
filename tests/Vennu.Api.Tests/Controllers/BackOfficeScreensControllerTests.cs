using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Controllers.BackOffice;
using Vennu.Api.Services;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public sealed class BackOfficeScreensControllerTests
{
    [Fact]
    public async Task Create_ReturnsConflict_WhenScreenLimitIsReached()
    {
        var controller = new BackOfficeScreensController(
            new RejectingScreenManagementService(),
            null!,
            null!,
            null!);

        var result = await controller.Create(
            Guid.NewGuid(),
            new ScreenCreateRequest("Extra screen", null),
            CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        var details = Assert.IsType<ProblemDetails>(conflict.Value);
        Assert.Equal(StatusCodes.Status409Conflict, details.Status);
        Assert.Equal("Screen limit reached.", details.Title);
    }

    private sealed class RejectingScreenManagementService : IScreenManagementService
    {
        public Task<ScreenManagementItem> CreateAsync(
            Guid venueId,
            string name,
            string? location,
            CancellationToken cancellationToken = default) =>
            Task.FromException<ScreenManagementItem>(new TierScreenLimitReachedException());

        public Task<IReadOnlyCollection<ScreenManagementItem>> GetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ScreenManagementItem?> UpdateAsync(
            Guid venueId,
            Guid screenId,
            string name,
            string? location,
            string? photoGridDensity,
            string? displayLayout,
            string? splitRatio = null,
            int? heroDwellSeconds = null,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> PushAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
