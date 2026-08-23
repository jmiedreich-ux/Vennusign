using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Vennu.Api.BackOffice;
using Vennu.Api.CustomerAuthentication;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.BackOffice;

// This handler gates every authenticated back-office request and had zero coverage.
// #800's real console timing showed a ~1.9s fixed floor on every write, traced here
// to sequential round trips: these tests lock in both perf fixes (skip the onboarding
// lookup when a venue header already resolves it; run the two membership lookups
// concurrently instead of in series) without changing observable authorization
// behavior - every case that used to authorize/refuse still does, identically.
[Trait("Category", "Unit")]
public sealed class CustomerBackOfficeAuthenticationHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid VenueId = Guid.NewGuid();
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private const string Token = "session-token";

    [Fact]
    public async Task NoSessionCookie_ReturnsNoResult()
    {
        var (handler, _) = await AuthenticateAsync(withCookie: false, headers: null);
        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Null(result.Failure);
    }

    [Fact]
    public async Task InvalidSession_Fails()
    {
        var sessions = new FakeSessionService { Identity = null };
        var (handler, _) = await BuildAsync(sessions, headers: null);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("The customer session is invalid or expired.", result.Failure!.Message);
    }

    [Fact]
    public async Task ExplicitVenueHeader_AuthorizesAndSkipsOnboardingLookup()
    {
        var onboarding = new FakeOnboardingRepository { Called = false };
        var (handler, _) = await BuildAsync(
            onboarding: onboarding,
            headers: new() { [BackOfficeAuthenticationDefaults.VenueSelectionHeaderName] = VenueId.ToString() });

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(VenueId.ToString(), result.Principal!.FindFirst(BackOfficeAuthenticationDefaults.VenueIdClaim)!.Value);
        Assert.False(onboarding.Called, "onboarding.GetByUserIdAsync should not run when a valid venue header already resolves the venue.");
    }

    [Fact]
    public async Task LegacyVenueHeader_StillWorks()
    {
        var (handler, _) = await BuildAsync(
            headers: new() { [BackOfficeAuthenticationDefaults.LegacyVenueSelectionHeaderName] = VenueId.ToString() });

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task NoHeader_FallsBackToOnboardingState()
    {
        var onboarding = new FakeOnboardingRepository { State = new CustomerOnboardingState { UserId = UserId, VenueId = VenueId } };
        var (handler, _) = await BuildAsync(onboarding: onboarding, headers: null);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.True(onboarding.Called);
    }

    [Fact]
    public async Task InvalidHeaderValue_FallsBackToOnboardingState()
    {
        var onboarding = new FakeOnboardingRepository { State = new CustomerOnboardingState { UserId = UserId, VenueId = VenueId } };
        var (handler, _) = await BuildAsync(
            onboarding: onboarding,
            headers: new() { [BackOfficeAuthenticationDefaults.VenueSelectionHeaderName] = "not-a-guid" });

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.True(onboarding.Called, "an unparseable header must still fall back to onboarding state, same as before.");
    }

    [Fact]
    public async Task ConflictingCanonicalAndLegacyHeaders_FailsAndSkipsOnboarding()
    {
        var onboarding = new FakeOnboardingRepository { Called = false };
        var (handler, _) = await BuildAsync(
            onboarding: onboarding,
            headers: new()
            {
                [BackOfficeAuthenticationDefaults.VenueSelectionHeaderName] = VenueId.ToString(),
                [BackOfficeAuthenticationDefaults.LegacyVenueSelectionHeaderName] = Guid.NewGuid().ToString()
            });

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("The customer is not authorized to manage the selected venue.", result.Failure!.Message);
        Assert.False(onboarding.Called, "conflicting headers never fell back to onboarding state before this change either.");
    }

    [Fact]
    public async Task NoHeaderAndNoOnboardingVenue_FallsBackToFirstAuthorizedContext()
    {
        var onboarding = new FakeOnboardingRepository { State = new CustomerOnboardingState { UserId = UserId, VenueId = null } };
        var contexts = new FakeContextRepository
        {
            Contexts = [new BackOfficeContextRecord { OrganizationId = OrganizationId, VenueId = VenueId }]
        };
        var (handler, _) = await BuildAsync(onboarding: onboarding, contexts: contexts, headers: null);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(VenueId.ToString(), result.Principal!.FindFirst(BackOfficeAuthenticationDefaults.VenueIdClaim)!.Value);
    }

    [Fact]
    public async Task NoCapability_Refuses()
    {
        var capabilities = new FakeCapabilityResolver { Grant = false };
        var (handler, _) = await BuildAsync(
            capabilities: capabilities,
            headers: new() { [BackOfficeAuthenticationDefaults.VenueSelectionHeaderName] = VenueId.ToString() });

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task RevokedMembership_IsTreatedAsNoRole()
    {
        var memberships = new FakeMembershipRepository
        {
            OrganizationMembership = new OrganizationMembership
            {
                OrganizationId = OrganizationId, UserId = UserId,
                Role = OrganizationMembershipRole.Owner, RevokedUtc = DateTime.UtcNow
            },
            VenueMembership = new VenueMembership
            {
                OrganizationId = OrganizationId, VenueId = VenueId, UserId = UserId,
                Role = VenueMembershipRole.Manager, RevokedUtc = DateTime.UtcNow
            }
        };
        var (handler, _) = await BuildAsync(
            memberships: memberships,
            headers: new() { [BackOfficeAuthenticationDefaults.VenueSelectionHeaderName] = VenueId.ToString() });

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded, "a revoked role must not authorize, even though the capability resolver would grant it unrevoked.");
    }

    [Fact]
    public async Task BothMembershipLookups_RunConcurrently()
    {
        var memberships = new FakeMembershipRepository { RecordConcurrency = true };
        var (handler, _) = await BuildAsync(
            memberships: memberships,
            headers: new() { [BackOfficeAuthenticationDefaults.VenueSelectionHeaderName] = VenueId.ToString() });

        await handler.AuthenticateAsync();

        Assert.True(memberships.BothWereInFlightTogether, "GetOrganizationMembershipAsync and GetVenueMembershipAsync should overlap, not run one after the other.");
    }

    private static Task<(CustomerBackOfficeAuthenticationHandler Handler, DefaultHttpContext Context)> AuthenticateAsync(
        bool withCookie, Dictionary<string, string>? headers) =>
        BuildAsync(headers: headers, includeCookie: withCookie);

    private static async Task<(CustomerBackOfficeAuthenticationHandler Handler, DefaultHttpContext Context)> BuildAsync(
        FakeSessionService? sessions = null,
        FakeOnboardingRepository? onboarding = null,
        FakeVenueRepository? venues = null,
        FakeMembershipRepository? memberships = null,
        FakeContextRepository? contexts = null,
        FakeCapabilityResolver? capabilities = null,
        Dictionary<string, string>? headers = null,
        bool includeCookie = true)
    {
        sessions ??= new FakeSessionService();
        onboarding ??= new FakeOnboardingRepository();
        venues ??= new FakeVenueRepository { GetByIdAsyncHandler = (_, _) => Task.FromResult<Venue?>(new Venue { Id = VenueId, OrganizationId = OrganizationId, Name = "Venue" }) };
        memberships ??= new FakeMembershipRepository();
        contexts ??= new FakeContextRepository();
        capabilities ??= new FakeCapabilityResolver();

        var handler = new CustomerBackOfficeAuthenticationHandler(
            new StaticOptionsMonitor<CustomerBackOfficeAuthenticationOptions>(new CustomerBackOfficeAuthenticationOptions()),
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            sessions, onboarding, venues, memberships, contexts, capabilities);

        var context = new DefaultHttpContext();
        if (includeCookie)
            context.Request.Headers.Append("Cookie", $"{CustomerAuthenticationDefaults.SessionCookieName}={Token}");
        if (headers is not null)
            foreach (var (key, value) in headers)
                context.Request.Headers[key] = value;

        var scheme = new AuthenticationScheme("BackOffice", null, typeof(CustomerBackOfficeAuthenticationHandler));
        await handler.InitializeAsync(scheme, context);
        return (handler, context);
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; } = value;
        public T Get(string? name) => CurrentValue;
        public IDisposable OnChange(Action<T, string?> listener) => new NoopDisposable();
        private sealed class NoopDisposable : IDisposable { public void Dispose() { } }
    }

    private sealed class FakeSessionService : ICustomerSessionService
    {
        public CustomerSessionIdentity? Identity { get; set; } = new(
            new CustomerAuthSession { Id = Guid.NewGuid(), UserId = UserId },
            new CustomerUser { Id = UserId, Email = "customer@example.com", DisplayName = "Customer" });

        public Task<CustomerSessionIssue> IssueAsync(Guid userId, CustomerAuthenticationMethod method, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerSessionIdentity?> AuthenticateAsync(string token, CancellationToken cancellationToken = default) =>
            Task.FromResult(Identity);
        public Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class FakeOnboardingRepository : ICustomerOnboardingRepository
    {
        public bool Called { get; set; }
        public CustomerOnboardingState? State { get; set; } = new() { UserId = UserId, VenueId = VenueId };

        public Task<CustomerOnboardingState?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(State);
        }
        public Task<CustomerOnboardingState> SaveAsync(CustomerOnboardingState state, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState?> GetByFirstScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState?> LatchGoLiveByFirstScreenAsync(Guid screenId, DateTime achievedUtc, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeMembershipRepository : IOrganizationMembershipRepository
    {
        public OrganizationMembership? OrganizationMembership { get; set; } =
            new() { OrganizationId = OrganizationId, UserId = UserId, Role = OrganizationMembershipRole.Owner };
        public VenueMembership? VenueMembership { get; set; } =
            new() { OrganizationId = OrganizationId, VenueId = VenueId, UserId = UserId, Role = VenueMembershipRole.Manager };

        public bool RecordConcurrency { get; set; }
        private int inFlight;
        public bool BothWereInFlightTogether { get; private set; }

        public Task<Organization?> GetOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public async Task<OrganizationMembership?> GetOrganizationMembershipAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken = default)
        {
            if (RecordConcurrency) await Track(cancellationToken);
            return OrganizationMembership;
        }

        public async Task<VenueMembership?> GetVenueMembershipAsync(Guid organizationId, Guid venueId, Guid userId, CancellationToken cancellationToken = default)
        {
            if (RecordConcurrency) await Track(cancellationToken);
            return VenueMembership;
        }

        private async Task Track(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref inFlight) == 2) BothWereInFlightTogether = true;
            await Task.Delay(20, cancellationToken);
            Interlocked.Decrement(ref inFlight);
        }

        public Task<Organization> CreateOrganizationAsync(Organization organization, OrganizationMembership ownerMembership, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<OrganizationMembership> SaveOrganizationMembershipAsync(OrganizationMembership membership, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task TransferOwnershipAsync(Guid organizationId, Guid currentOwnerUserId, Guid newOwnerUserId, DateTime occurredUtc, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task AttachVenueAsync(Guid organizationId, Guid venueId, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<VenueMembership> SaveVenueMembershipAsync(VenueMembership membership, MembershipAuditEntry auditEntry, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeContextRepository : IBackOfficeContextRepository
    {
        public IReadOnlyCollection<BackOfficeContextRecord> Contexts { get; set; } = [];
        public Task<IReadOnlyCollection<BackOfficeContextRecord>> GetAuthorizedAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Contexts);
    }

    private sealed class FakeCapabilityResolver : IMembershipCapabilityResolver
    {
        public bool Grant { get; set; } = true;
        public IReadOnlySet<MembershipCapability> Resolve(OrganizationMembershipRole? organizationRole, VenueMembershipRole? venueRole) =>
            throw new NotSupportedException();
        // A null role (no membership, or one already stripped by RevokedUtc) can never carry a
        // capability in reality - mirror that here rather than letting Grant alone decide, or a
        // revoked-membership test would "authorize" with no role and crash ResolveSystemRole.
        public bool HasCapability(OrganizationMembershipRole? organizationRole, VenueMembershipRole? venueRole, MembershipCapability capability) =>
            Grant && (organizationRole is not null || venueRole is not null);
    }
}
