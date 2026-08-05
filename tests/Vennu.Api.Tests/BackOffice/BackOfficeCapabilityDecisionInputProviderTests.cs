using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.BackOffice;

[Trait("Category", "Unit")]
public sealed class BackOfficeCapabilityDecisionInputProviderTests
{
    private static readonly Guid OrganizationId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Pairing_IsDeniedWhenTypedAllowanceIsReached()
    {
        var capability = CapabilityId.Parse("screen.device.pair");
        var provider = CreateProvider(new CapabilityAccessPolicy(
            capability, CapabilityRolloutState.Available, true, true, 1, 1));

        var input = await provider.ResolveAsync(capability, "pair-limit", "en-US");
        var result = new CapabilityDecisionEngine().Evaluate(input);

        Assert.Equal(CapabilityDecisionOutcome.Denied, result.Decision);
        Assert.Equal(CapabilityDecisionCategory.Allowance, result.Category);
        Assert.Equal("allowance.reached", result.ReasonCode);
    }

    [Theory]
    [InlineData("publishing.delivery.retry")]
    [InlineData("publishing.delivery.restore")]
    [InlineData("screen.delivery.recover")]
    [InlineData("publishing.release.unpublish")]
    public async Task CorrectionAndRecovery_RemainAllowedAtAllowanceLimit(string capabilityValue)
    {
        var capability = CapabilityId.Parse(capabilityValue);
        var provider = CreateProvider(new CapabilityAccessPolicy(
            capability, CapabilityRolloutState.Available, true, true, 1, 1));

        var input = await provider.ResolveAsync(capability, "recover", "en-US");
        var result = new CapabilityDecisionEngine().Evaluate(input);

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public async Task TemporaryRollout_ReturnsRetryableStructuredReason()
    {
        var capability = CapabilityId.Parse("content.item.update");
        var provider = CreateProvider(new CapabilityAccessPolicy(
            capability,
            CapabilityRolloutState.TemporarilyBlocked,
            true,
            true,
            null,
            0,
            new DateTime(2026, 8, 5, 21, 10, 0, DateTimeKind.Utc)));

        var input = await provider.ResolveAsync(capability, "temporary", "fr-CA");
        var result = new CapabilityDecisionEngine().Evaluate(input);

        Assert.Equal(CapabilityDecisionOutcome.TemporarilyBlocked, result.Decision);
        Assert.Equal("rollout.temporarily_blocked", result.ReasonCode);
        Assert.Equal(TimeSpan.FromMinutes(10), result.RetryAfter);
        Assert.Equal("fr-CA", result.Locale);
    }

    private static BackOfficeCapabilityDecisionInputProvider CreateProvider(CapabilityAccessPolicy policy)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "33333333-3333-3333-3333-333333333333"),
            new Claim(BackOfficeAuthenticationDefaults.OrganizationIdClaim, OrganizationId.ToString()),
            new Claim(BackOfficeAuthenticationDefaults.VenueIdClaim, VenueId.ToString()),
            new Claim(BackOfficeAuthenticationDefaults.SystemRoleClaim, "organization_owner")
        };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        };
        return new BackOfficeCapabilityDecisionInputProvider(
            new HttpContextAccessor { HttpContext = context },
            new FixedPolicyRepository(policy),
            new EmptyScopedAuthorityRepository(),
            new ScopedPermissionEvaluator(),
            new FixedTimeProvider());
    }

    private sealed class EmptyScopedAuthorityRepository : IScopedAuthorityRepository
    {
        public Task<IReadOnlyCollection<ScopedRoleAssignment>> GetActiveAssignmentsAsync(Guid actorUserId, DateTime utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<ScopedRoleAssignment>>([]);
        public Task SaveAssignmentAsync(ScopedRoleAssignment assignment, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<SupportAccessGrant?> GetActiveSupportGrantAsync(Guid supportUserId, Guid organizationId, Guid? venueId, DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult<SupportAccessGrant?>(null);
        public Task SaveSupportGrantAsync(SupportAccessGrant grant, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AppendSupportAuditAsync(SupportAccessAuditEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FixedPolicyRepository(CapabilityAccessPolicy policy) : ICapabilityAccessPolicyRepository
    {
        public Task<CapabilityAccessPolicy> GetAsync(
            Guid organizationId,
            Guid venueId,
            CapabilityId capability,
            DateTime utcNow,
            CancellationToken cancellationToken = default) => Task.FromResult(policy);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 8, 5, 21, 0, 0, TimeSpan.Zero);
    }
}
