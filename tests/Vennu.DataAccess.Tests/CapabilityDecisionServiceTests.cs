using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public sealed class CapabilityDecisionServiceTests
{
    [Fact]
    public async Task ActionAuthorizer_ReevaluatesImmediatelyBeforeEveryMutation()
    {
        var capability = CapabilityId.Parse("content.item.availability_update");
        var provider = new CountingProvider();
        var decisions = new CapabilityDecisionService(provider, new CapabilityDecisionEngine());
        var authorizer = new CapabilityActionAuthorizer(decisions);

        await authorizer.RequireAllowedAsync(capability, "first", "en-US");
        await authorizer.RequireAllowedAsync(capability, "second", "en-US");

        Assert.Equal(2, provider.SingleResolutionCount);
    }

    [Fact]
    public async Task ActionAuthorizer_ThrowsStructuredDecisionWhenBlocked()
    {
        var capability = CapabilityId.Parse("publishing.release.publish");
        var provider = new CountingProvider(permissionAllowed: false);
        var authorizer = new CapabilityActionAuthorizer(
            new CapabilityDecisionService(provider, new CapabilityDecisionEngine()));

        var error = await Assert.ThrowsAsync<CapabilityDecisionDeniedException>(() =>
            authorizer.RequireAllowedAsync(capability, "blocked", "fr-CA"));

        Assert.Equal(CapabilityDecisionCategory.Permission, error.Decision.Category);
        Assert.Equal("permission.required", error.Decision.ReasonCode);
        Assert.Equal("fr-CA", error.Decision.Locale);
    }

    [Fact]
    public async Task BatchEvaluation_UsesOneBatchResolution()
    {
        var provider = new CountingProvider();
        var service = new CapabilityDecisionService(provider, new CapabilityDecisionEngine());
        CapabilityId[] capabilities =
        [
            CapabilityId.Parse("screen.device.view"),
            CapabilityId.Parse("analytics.delivery_health.view")
        ];

        var results = await service.EvaluateBatchAsync(capabilities, "batch", "en-US");

        Assert.Equal(0, provider.SingleResolutionCount);
        Assert.Equal(1, provider.BatchResolutionCount);
        Assert.Equal(capabilities, results.Select(item => item.Capability));
    }

    private sealed class CountingProvider(bool permissionAllowed = true) : ICapabilityDecisionInputProvider
    {
        public int SingleResolutionCount { get; private set; }
        public int BatchResolutionCount { get; private set; }

        public Task<CapabilityDecisionInput> ResolveAsync(
            CapabilityId capability,
            string correlationId,
            string locale,
            CancellationToken cancellationToken = default)
        {
            SingleResolutionCount++;
            return Task.FromResult(Create(capability, correlationId, locale));
        }

        public Task<IReadOnlyCollection<CapabilityDecisionInput>> ResolveBatchAsync(
            IReadOnlyCollection<CapabilityId> capabilities,
            string correlationId,
            string locale,
            CancellationToken cancellationToken = default)
        {
            BatchResolutionCount++;
            return Task.FromResult<IReadOnlyCollection<CapabilityDecisionInput>>(
                capabilities.Select(capability => Create(capability, correlationId, locale)).ToArray());
        }

        private CapabilityDecisionInput Create(CapabilityId capability, string correlationId, string locale)
        {
            var dimensions = Enum.GetValues<CapabilityDecisionCategory>()
                .Where(category => category is not CapabilityDecisionCategory.None and not CapabilityDecisionCategory.Capability)
                .Select(category => category == CapabilityDecisionCategory.Permission && !permissionAllowed
                    ? CapabilityDecisionDimension.Failed(
                        category,
                        CapabilityDecisionOutcome.Denied,
                        "permission.required",
                        "decisions.permission.required")
                    : CapabilityDecisionDimension.Satisfied(category))
                .ToArray();
            return new CapabilityDecisionInput(capability, dimensions, correlationId, locale);
        }
    }
}
