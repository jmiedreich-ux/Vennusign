# Keystone Milestone 1 — TenantContext Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the TenantContext contract and its library into `Vennu.Api` so that a request can carry which tenant it is *about*, with nothing yet depending on it.

**Architecture:** A new dependency-free `Vennu.Tenancy` project owns the contract in two layers — the public one is a URL path prefix (`/o/{orgId}/v/{venueId}/…`), the internal one is an ES256-signed token minted by the Product Router. `Vennu.Api` gains a middleware that resolves whichever is present, strips the prefix so existing route templates are untouched, and exposes the result through DI. Nothing reads it yet, so the whole milestone is inert and reversible.

**Tech Stack:** .NET 9, xunit 2.9.2, BCL only. **No new package references.** ES256 is implemented with `System.Security.Cryptography.ECDsa` and `System.Text.Json` deliberately: decision 2 requires the thin layer's change frequency to stay low, and decision 18 makes the wire format additive-only forever, so the format is owned outright rather than inherited from a library's serialization choices and version cadence.

**Spec:** `docs/design/proposed/keystone/decisions.md` (49 decisions). Answers that shaped this plan: `docs/features/keystone/open-questions.md`.

## Milestone discipline

This is a numbered milestone under AGENTS.md's working model, not a loose batch of work.
Before starting: create the milestone issue, record the claim in `tracker/assignments.json`,
and branch as `feature/keystone-m1-<short-name>` from merged `master`. One PR. Verify locally
(CI is suspended by owner decision — local checks *are* the gate). Obtain independent review,
never by the author. Merge, then synchronize `PROJECT_STATUS.md`, the tracker,
`ai/handoffs/current.md` and this feature's records.

**Ends with a short owner acceptance workbook** (5–10 minutes) before the next milestone starts.
A milestone that ships no UI gets a demo script instead. Only one milestone runs at a time.

## Governance gate — read before starting

**This plan must not be executed yet.** The design authority is in `docs/design/proposed/`, not `approved/`. AGENTS.md requires an approved design authority before implementation, and the brainstorming skill's gate is unmet until the owner approves the spec. Executing this plan before that approval is out of order regardless of how ready the tasks look.

**Orchestrator-owned files are touched.** Per AGENTS.md's multi-agent safety rules, this plan modifies `Vennusign.sln`, a `.csproj`, and dependency injection in `Program.cs`. Those are orchestrator-owned; do not run two agents against this plan concurrently.

**CI is suspended.** Local verification is the gate. Every task below states the exact command and the expected result.

## Global Constraints

- **Target `.NET 9`.** `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` on every project, matching `src/Vennu.Core.Models/Vennu.Core.Models.csproj`.
- **No new package references anywhere in this milestone.**
- **Decision 11 — hint and authority are separate sources, always.** Nothing produced by this milestone may be used for an authorization decision. Reviewers should reject any use of `TenantContext` in an authorization path.
- **Decision 14 — the API's route templates do not change.** `BackOfficeMenusController` stays at `api/back-office/menus`. The prefix is stripped before routing.
- **Decision 18 — the wire format is additive-only, permanently.** No field may ever be removed or reinterpreted; only added, in ways an old parser ignores. Unknown JSON members are ignored on read, never rejected.
- **Decision 32 — tokens are asymmetric, audience-scoped and short-lived.** ES256; audience is the version the token was minted for; TTL 60 seconds (register Q16).
- **Decision 33 — a token records how the tenant was established**, not only what it is.
- **Backward compatibility is the acceptance bar.** A request with neither prefix nor token must behave exactly as it does today.

## File Structure

| File | Responsibility |
|---|---|
| `src/Vennu.Tenancy/Vennu.Tenancy.csproj` | New project. No package references, no project references. |
| `src/Vennu.Tenancy/TenantContext.cs` | The contract type and its provenance enum. |
| `src/Vennu.Tenancy/TenantPath.cs` | Public contract: parse and format the URL prefix. Pure. |
| `src/Vennu.Tenancy/TenantToken.cs` | Internal contract: ES256 issue and verify. Pure apart from the clock. |
| `src/Vennu.Api/Infrastructure/TenantContextAccessor.cs` | DI surface for reading the resolved context. |
| `src/Vennu.Api/Infrastructure/TenantContextMiddleware.cs` | Resolve token, else prefix, else nothing. Strip the prefix. |
| `tests/Vennu.Tenancy.Tests/` | Unit tests for the three pure units. |
| `tests/Vennu.Api.Tests/Infrastructure/TenantContextMiddlewareTests.cs` | Middleware behaviour, including the untouched-request case. |

`Vennu.Tenancy` takes no project references on purpose. It is consumed by `Vennu.Api` now and by the Product Router later, and those are separate deployables — so it must not drag Vennusign domain types across that boundary.

---

### Task 1: The `Vennu.Tenancy` project and the `TenantContext` type

**Files:**
- Create: `src/Vennu.Tenancy/Vennu.Tenancy.csproj`
- Create: `src/Vennu.Tenancy/TenantContext.cs`
- Create: `tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj`
- Create: `tests/Vennu.Tenancy.Tests/TenantContextTests.cs`
- Modify: `Vennusign.sln`

**Interfaces:**
- Consumes: nothing.
- Produces: `Vennu.Tenancy.TenantProvenance` (enum: `Asserted`, `Verified`); `Vennu.Tenancy.TenantContext` (sealed record with `Guid VenueId`, `Guid? OrganizationId`, `TenantProvenance Provenance`).

- [ ] **Step 1: Write the failing test**

Create `tests/Vennu.Tenancy.Tests/TenantContextTests.cs`:

```csharp
using Vennu.Tenancy;

namespace Vennu.Tenancy.Tests;

public sealed class TenantContextTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Asserted_IsTheDefaultProvenance()
    {
        // Decision 11: anything the Router read from a caller is a hint until
        // something else establishes it. Asserted must be the zero value so a
        // context constructed carelessly is never accidentally trusted.
        Assert.Equal(TenantProvenance.Asserted, default(TenantProvenance));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CarriesVenueOrganizationAndProvenance()
    {
        var venue = Guid.NewGuid();
        var org = Guid.NewGuid();

        var context = new TenantContext(venue, org, TenantProvenance.Verified);

        Assert.Equal(venue, context.VenueId);
        Assert.Equal(org, context.OrganizationId);
        Assert.Equal(TenantProvenance.Verified, context.Provenance);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void OrganizationIsOptional()
    {
        // Decision 25: the Router only ever keys on venue. The org segment is for
        // the application, so a context is well-formed without it.
        var context = new TenantContext(Guid.NewGuid(), null, TenantProvenance.Asserted);
        Assert.Null(context.OrganizationId);
    }
}
```

- [ ] **Step 2: Create the projects and register them in the solution**

Create `src/Vennu.Tenancy/Vennu.Tenancy.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>

</Project>
```

Create `tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Vennu.Tenancy\Vennu.Tenancy.csproj" />
  </ItemGroup>
</Project>
```

Add both to the solution:

```bash
dotnet sln Vennusign.sln add src/Vennu.Tenancy/Vennu.Tenancy.csproj --solution-folder src
dotnet sln Vennusign.sln add tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj --solution-folder tests
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj`
Expected: FAIL to compile — `The type or namespace name 'TenantContext' could not be found`.

- [ ] **Step 4: Write the minimal implementation**

Create `src/Vennu.Tenancy/TenantContext.cs`:

```csharp
namespace Vennu.Tenancy;

/// <summary>
/// How the tenant on a request was established. Decision 33: the receiving version
/// must be able to tell a verified fact from a caller's assertion.
/// </summary>
public enum TenantProvenance
{
    /// <summary>The caller said so and nothing checked it. The default, deliberately.</summary>
    Asserted = 0,

    /// <summary>Established by something that verified it, such as a POS provider signature.</summary>
    Verified = 1
}

/// <summary>
/// Which tenant a request is <em>about</em> — not who is making it. Decision 10.
/// Decision 11: this is a routing input only and must never reach an authorization decision.
/// </summary>
public sealed record TenantContext(
    Guid VenueId,
    Guid? OrganizationId,
    TenantProvenance Provenance);
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj`
Expected: PASS, 3 of 3.

- [ ] **Step 6: Commit**

```bash
git add src/Vennu.Tenancy tests/Vennu.Tenancy.Tests Vennusign.sln
git commit -m "feat(tenancy): add the TenantContext contract type

Decision 10 names the subject rather than the caller. Provenance defaults to
Asserted so a carelessly constructed context is never accidentally trusted."
```

---

### Task 2: `TenantPath` — the public contract

**Files:**
- Create: `src/Vennu.Tenancy/TenantPath.cs`
- Create: `tests/Vennu.Tenancy.Tests/TenantPathTests.cs`

**Interfaces:**
- Consumes: `TenantContext`, `TenantProvenance` from Task 1.
- Produces: `static bool TenantPath.TryParse(string path, out TenantContext? context, out string remainder)` and `static string TenantPath.Format(TenantContext context, string remainder)`.

- [ ] **Step 1: Write the failing test**

Create `tests/Vennu.Tenancy.Tests/TenantPathTests.cs`:

```csharp
using Vennu.Tenancy;

namespace Vennu.Tenancy.Tests;

public sealed class TenantPathTests
{
    private static readonly Guid Org = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Venue = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    [Trait("Category", "Unit")]
    public void ParsesOrgAndVenueAndReturnsTheBarePath()
    {
        var ok = TenantPath.TryParse($"/o/{Org}/v/{Venue}/api/back-office/menus", out var context, out var remainder);

        Assert.True(ok);
        Assert.Equal(Venue, context!.VenueId);
        Assert.Equal(Org, context.OrganizationId);
        // Decision 14: what reaches routing is the bare path, unchanged.
        Assert.Equal("/api/back-office/menus", remainder);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ParsedContextIsAlwaysAsserted()
    {
        // Decision 11: a path segment is caller-supplied. Parsing it never upgrades it.
        TenantPath.TryParse($"/o/{Org}/v/{Venue}/api/x", out var context, out _);
        Assert.Equal(TenantProvenance.Asserted, context!.Provenance);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void AcceptsAVenueWithoutAnOrganizationSegment()
    {
        var ok = TenantPath.TryParse($"/v/{Venue}/api/x", out var context, out var remainder);

        Assert.True(ok);
        Assert.Equal(Venue, context!.VenueId);
        Assert.Null(context.OrganizationId);
        Assert.Equal("/api/x", remainder);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void PrefixWithNoTrailingPathYieldsRoot()
    {
        var ok = TenantPath.TryParse($"/o/{Org}/v/{Venue}", out _, out var remainder);

        Assert.True(ok);
        Assert.Equal("/", remainder);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/api/back-office/menus")]          // no prefix at all — today's traffic
    [InlineData("/signin")]                          // pre-auth root route, decision 19
    [InlineData("/pair")]                            // pre-auth device route, decision 19
    [InlineData("/o/not-a-guid/v/also-not/api/x")]   // malformed identifiers
    [InlineData("/v/not-a-guid/api/x")]
    [InlineData("/o/11111111-1111-1111-1111-111111111111/api/x")] // org without venue
    [InlineData("")]
    public void LeavesAnythingWithoutAValidPrefixAlone(string path)
    {
        var ok = TenantPath.TryParse(path, out var context, out var remainder);

        Assert.False(ok);
        Assert.Null(context);
        Assert.Equal(path, remainder);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void FormatIsTheInverseOfParse()
    {
        var original = $"/o/{Org}/v/{Venue}/api/back-office/menus";
        TenantPath.TryParse(original, out var context, out var remainder);

        Assert.Equal(original, TenantPath.Format(context!, remainder));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void FormatOmitsTheOrganizationSegmentWhenThereIsNone()
    {
        var context = new TenantContext(Venue, null, TenantProvenance.Asserted);
        Assert.Equal($"/v/{Venue}/api/x", TenantPath.Format(context, "/api/x"));
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj --filter FullyQualifiedName~TenantPathTests`
Expected: FAIL to compile — `The name 'TenantPath' does not exist`.

- [ ] **Step 3: Write the implementation**

Create `src/Vennu.Tenancy/TenantPath.cs`:

```csharp
namespace Vennu.Tenancy;

/// <summary>
/// The public half of the contract (decision 13): the tenant travels in the URL path,
/// so a relative call from a bundle inherits it with no client code.
/// Decision 18: this shape is additive-only and may never be reinterpreted.
/// </summary>
public static class TenantPath
{
    private const string OrgSegment = "o";
    private const string VenueSegment = "v";

    /// <summary>
    /// Reads a tenant prefix off the front of <paramref name="path"/>.
    /// Returns false and leaves <paramref name="remainder"/> as the original path when
    /// there is no well-formed prefix — which is the normal case for pre-auth routes and
    /// for every request that exists today.
    /// </summary>
    public static bool TryParse(string path, out TenantContext? context, out string remainder)
    {
        context = null;
        remainder = path;

        if (string.IsNullOrEmpty(path) || path[0] != '/') return false;

        var parts = path.Split('/', StringSplitOptions.None);
        // parts[0] is empty because the path starts with '/'.
        var index = 1;
        Guid? organizationId = null;

        if (parts.Length > index + 1 && parts[index] == OrgSegment)
        {
            if (!Guid.TryParse(parts[index + 1], out var org)) return false;
            organizationId = org;
            index += 2;
        }

        if (parts.Length <= index + 1 || parts[index] != VenueSegment) return false;
        if (!Guid.TryParse(parts[index + 1], out var venue)) return false;
        index += 2;

        context = new TenantContext(venue, organizationId, TenantProvenance.Asserted);
        var rest = string.Join('/', parts.Skip(index));
        remainder = rest.Length == 0 ? "/" : "/" + rest;
        return true;
    }

    /// <summary>Builds a tenant-prefixed path. The inverse of <see cref="TryParse"/>.</summary>
    public static string Format(TenantContext context, string remainder)
    {
        var tail = string.IsNullOrEmpty(remainder) ? "/" : remainder;
        if (tail[0] != '/') tail = "/" + tail;
        if (tail == "/") tail = string.Empty;

        return context.OrganizationId is { } org
            ? $"/{OrgSegment}/{org}/{VenueSegment}/{context.VenueId}{tail}"
            : $"/{VenueSegment}/{context.VenueId}{tail}";
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj`
Expected: PASS, 13 of 13 (3 from Task 1, 10 here — the `[Theory]` contributes 7).

- [ ] **Step 5: Commit**

```bash
git add src/Vennu.Tenancy/TenantPath.cs tests/Vennu.Tenancy.Tests/TenantPathTests.cs
git commit -m "feat(tenancy): parse and format the tenant path prefix

Decision 13 makes the URL the public contract. Parsing never upgrades
provenance past Asserted, and anything without a well-formed prefix is
returned untouched so today's traffic is unaffected."
```

---

### Task 3: `TenantToken` — the internal contract

**Files:**
- Create: `src/Vennu.Tenancy/TenantToken.cs`
- Create: `tests/Vennu.Tenancy.Tests/TenantTokenTests.cs`

**Interfaces:**
- Consumes: `TenantContext`, `TenantProvenance` from Task 1.
- Produces: `TenantTokenIssuer(ECDsa signingKey, string keyId, TimeProvider clock)` with `string Issue(TenantContext context, string audienceVersion, TimeSpan lifetime)`; `TenantTokenVerifier(ECDsa publicKey, string audienceVersion, TimeProvider clock)` with `bool TryVerify(string token, out TenantContext? context, out string? failure)`.

`TimeProvider` is taken as a constructor dependency so expiry is testable without sleeping. Use `TimeProvider.System` in production and `FakeTimeProvider`-style stubs in tests — a hand-written stub is used here to avoid a package reference.

- [ ] **Step 1: Write the failing test**

Create `tests/Vennu.Tenancy.Tests/TenantTokenTests.cs`:

```csharp
using System.Security.Cryptography;
using Vennu.Tenancy;

namespace Vennu.Tenancy.Tests;

public sealed class TenantTokenTests
{
    private sealed class StubClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static readonly Guid Venue = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Org = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string Version = "1.5.0";

    private static (TenantTokenIssuer Issuer, ECDsa Key, StubClock Clock) NewIssuer()
    {
        var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var clock = new StubClock(new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero));
        return (new TenantTokenIssuer(key, "k1", clock), key, clock);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RoundTripsAContext()
    {
        var (issuer, key, clock) = NewIssuer();
        var token = issuer.Issue(new TenantContext(Venue, Org, TenantProvenance.Verified), Version, TimeSpan.FromSeconds(60));

        var verifier = new TenantTokenVerifier(key, Version, clock);
        Assert.True(verifier.TryVerify(token, out var context, out var failure));

        Assert.Null(failure);
        Assert.Equal(Venue, context!.VenueId);
        Assert.Equal(Org, context.OrganizationId);
        Assert.Equal(TenantProvenance.Verified, context.Provenance);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RejectsATokenMintedForAnotherVersion()
    {
        // Decision 32: audience-scoped, so a token for v1.4 cannot be replayed at v1.5.
        var (issuer, key, clock) = NewIssuer();
        var token = issuer.Issue(new TenantContext(Venue, Org, TenantProvenance.Asserted), "1.4.0", TimeSpan.FromSeconds(60));

        var verifier = new TenantTokenVerifier(key, "1.5.0", clock);

        Assert.False(verifier.TryVerify(token, out var context, out var failure));
        Assert.Null(context);
        Assert.Equal("audience", failure);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RejectsAnExpiredToken()
    {
        var (issuer, key, clock) = NewIssuer();
        var token = issuer.Issue(new TenantContext(Venue, null, TenantProvenance.Asserted), Version, TimeSpan.FromSeconds(60));

        clock.Now = clock.Now.AddSeconds(61);
        var verifier = new TenantTokenVerifier(key, Version, clock);

        Assert.False(verifier.TryVerify(token, out _, out var failure));
        Assert.Equal("expired", failure);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RejectsATokenSignedByAnotherKey()
    {
        // Decision 32: asymmetric, so a compromised API version cannot forge Router tokens.
        var (issuer, _, clock) = NewIssuer();
        var token = issuer.Issue(new TenantContext(Venue, null, TenantProvenance.Asserted), Version, TimeSpan.FromSeconds(60));

        using var stranger = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var verifier = new TenantTokenVerifier(stranger, Version, clock);

        Assert.False(verifier.TryVerify(token, out _, out var failure));
        Assert.Equal("signature", failure);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RejectsATamperedPayload()
    {
        var (issuer, key, clock) = NewIssuer();
        var token = issuer.Issue(new TenantContext(Venue, null, TenantProvenance.Asserted), Version, TimeSpan.FromSeconds(60));

        var parts = token.Split('.');
        var forged = new TenantContext(Guid.NewGuid(), null, TenantProvenance.Verified);
        var tampered = issuer.Issue(forged, Version, TimeSpan.FromSeconds(60)).Split('.')[1];
        var attack = $"{parts[0]}.{tampered}.{parts[2]}";

        var verifier = new TenantTokenVerifier(key, Version, clock);

        Assert.False(verifier.TryVerify(attack, out _, out var failure));
        Assert.Equal("signature", failure);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("not-a-token")]
    [InlineData("only.two")]
    [InlineData("a.b.c.d")]
    public void RejectsMalformedInput(string token)
    {
        var (_, key, clock) = NewIssuer();
        var verifier = new TenantTokenVerifier(key, Version, clock);

        Assert.False(verifier.TryVerify(token, out _, out var failure));
        Assert.Equal("malformed", failure);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IgnoresUnknownMembersRatherThanRejecting()
    {
        // Decision 18: additive-only, forever. A future field must not break an
        // older live version that has never heard of it.
        var (issuer, key, clock) = NewIssuer();
        var token = issuer.Issue(new TenantContext(Venue, Org, TenantProvenance.Asserted), Version, TimeSpan.FromSeconds(60));

        var parts = token.Split('.');
        var payload = System.Text.Encoding.UTF8.GetString(TenantToken.Base64UrlDecode(parts[1]));
        var widened = payload.Insert(payload.Length - 1, ",\"future\":\"value\"");
        var rebuilt = TenantToken.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(widened));

        // Re-sign so only the unknown member is under test, not the signature.
        var resigned = issuer.SignParts(parts[0], rebuilt);

        var verifier = new TenantTokenVerifier(key, Version, clock);
        Assert.True(verifier.TryVerify($"{parts[0]}.{rebuilt}.{resigned}", out var context, out _));
        Assert.Equal(Venue, context!.VenueId);
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj --filter FullyQualifiedName~TenantTokenTests`
Expected: FAIL to compile — `The name 'TenantTokenIssuer' does not exist`.

- [ ] **Step 3: Write the implementation**

Create `src/Vennu.Tenancy/TenantToken.cs`:

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vennu.Tenancy;

/// <summary>
/// The internal half of the contract (decisions 31–34). A compact ES256-signed
/// envelope minted by the Product Router and verified by the receiving version.
///
/// Implemented on the BCL alone, deliberately: decision 2 requires this layer's
/// change frequency to stay low, and decision 18 makes the format additive-only
/// forever, so it is owned outright rather than inherited from a library.
///
/// Decision 34: this authenticates the hop, not the claim. A verified signature
/// proves the Router said it; it does not make the tenant true, and nothing here
/// may be used for an authorization decision.
/// </summary>
public static class TenantToken
{
    internal const string HeaderType = "vennu-tenant+jwt";

    public static string Base64UrlEncode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static byte[] Base64UrlDecode(string value)
    {
        var s = value.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(s.PadRight(s.Length + (4 - s.Length % 4) % 4, '='));
    }

    internal sealed record Payload(
        [property: JsonPropertyName("ven")] Guid Venue,
        [property: JsonPropertyName("org")] Guid? Organization,
        [property: JsonPropertyName("prv")] string Provenance,
        [property: JsonPropertyName("aud")] string Audience,
        [property: JsonPropertyName("iat")] long IssuedAt,
        [property: JsonPropertyName("exp")] long ExpiresAt);

    internal static readonly JsonSerializerOptions Json = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
}

/// <summary>Mints tokens. Held only by the Product Router.</summary>
public sealed class TenantTokenIssuer(ECDsa signingKey, string keyId, TimeProvider clock)
{
    public string Issue(TenantContext context, string audienceVersion, TimeSpan lifetime)
    {
        var now = clock.GetUtcNow().ToUnixTimeSeconds();
        var header = TenantToken.Base64UrlEncode(Encoding.UTF8.GetBytes(
            $"{{\"alg\":\"ES256\",\"typ\":\"{TenantToken.HeaderType}\",\"kid\":\"{keyId}\"}}"));

        var payload = TenantToken.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(
            new TenantToken.Payload(
                context.VenueId,
                context.OrganizationId,
                context.Provenance == TenantProvenance.Verified ? "verified" : "asserted",
                audienceVersion,
                now,
                now + (long)lifetime.TotalSeconds),
            TenantToken.Json));

        return $"{header}.{payload}.{SignParts(header, payload)}";
    }

    /// <summary>Signs an already-encoded header and payload. Exposed for tests that widen the payload.</summary>
    public string SignParts(string encodedHeader, string encodedPayload)
    {
        var signature = signingKey.SignData(
            Encoding.ASCII.GetBytes($"{encodedHeader}.{encodedPayload}"),
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return TenantToken.Base64UrlEncode(signature);
    }
}

/// <summary>Verifies tokens. Held by every concurrently running version.</summary>
public sealed class TenantTokenVerifier(ECDsa publicKey, string audienceVersion, TimeProvider clock)
{
    public bool TryVerify(string token, out TenantContext? context, out string? failure)
    {
        context = null;

        var parts = token.Split('.');
        if (parts.Length != 3 || parts.Any(string.IsNullOrEmpty))
        {
            failure = "malformed";
            return false;
        }

        byte[] signature;
        TenantToken.Payload? payload;
        try
        {
            signature = TenantToken.Base64UrlDecode(parts[2]);
            payload = JsonSerializer.Deserialize<TenantToken.Payload>(
                TenantToken.Base64UrlDecode(parts[1]), TenantToken.Json);
        }
        catch (Exception e) when (e is FormatException or JsonException)
        {
            failure = "malformed";
            return false;
        }

        if (payload is null)
        {
            failure = "malformed";
            return false;
        }

        var verified = publicKey.VerifyData(
            Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}"),
            signature,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        if (!verified)
        {
            failure = "signature";
            return false;
        }

        if (!string.Equals(payload.Audience, audienceVersion, StringComparison.Ordinal))
        {
            failure = "audience";
            return false;
        }

        if (clock.GetUtcNow().ToUnixTimeSeconds() >= payload.ExpiresAt)
        {
            failure = "expired";
            return false;
        }

        context = new TenantContext(
            payload.Venue,
            payload.Organization,
            payload.Provenance == "verified" ? TenantProvenance.Verified : TenantProvenance.Asserted);
        failure = null;
        return true;
    }
}
```

Note the check order: signature before audience before expiry. A tampered token must report `signature` rather than leaking which other field an attacker got wrong.

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj`
Expected: PASS, 23 of 23.

- [ ] **Step 5: Commit**

```bash
git add src/Vennu.Tenancy/TenantToken.cs tests/Vennu.Tenancy.Tests/TenantTokenTests.cs
git commit -m "feat(tenancy): ES256 tenant token, issued and verified on the BCL alone

Decisions 31-34. Asymmetric so a compromised version cannot forge Router
tokens, audience-scoped so a v1.4 token cannot be replayed at v1.5, and
additive-only so an unknown future member is ignored rather than rejected.
No package reference: decision 2 keeps this layer's change frequency low."
```

---

### Task 4: Resolving the context inside `Vennu.Api`

**Files:**
- Create: `src/Vennu.Api/Infrastructure/TenantContextAccessor.cs`
- Create: `src/Vennu.Api/Infrastructure/TenantContextMiddleware.cs`
- Create: `tests/Vennu.Api.Tests/Infrastructure/TenantContextMiddlewareTests.cs`
- Modify: `src/Vennu.Api/Vennu.Api.csproj`
- Modify: `tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj`

**Interfaces:**
- Consumes: `TenantContext`, `TenantPath`, `TenantTokenVerifier` from Tasks 1–3.
- Produces: `ITenantContextAccessor` with `TenantContext? Current { get; }`; `TenantContextMiddleware`; `TenantContextMiddleware.HeaderName` (`"X-Vennusign-Tenant-Token"`).

Resolution order is token, then path, then nothing. A verified token wins because the Router strips the prefix before forwarding (decision 14), so in production both are never present; the path branch exists so the contract works before the Router does.

- [ ] **Step 1: Add the project reference to both csproj files**

In `src/Vennu.Api/Vennu.Api.csproj`, inside the existing `ItemGroup` holding `ProjectReference` entries, add:

```xml
    <ProjectReference Include="..\Vennu.Tenancy\Vennu.Tenancy.csproj" />
```

In `tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj`, inside the `ItemGroup` holding `ProjectReference` entries, add:

```xml
    <ProjectReference Include="..\..\src\Vennu.Tenancy\Vennu.Tenancy.csproj" />
```

- [ ] **Step 2: Write the failing test**

Create `tests/Vennu.Api.Tests/Infrastructure/TenantContextMiddlewareTests.cs`:

```csharp
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Vennu.Api.Infrastructure;
using Vennu.Tenancy;

namespace Vennu.Api.Tests.Infrastructure;

public sealed class TenantContextMiddlewareTests
{
    private sealed class StubClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static readonly Guid Venue = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Org = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly StubClock Clock = new(new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero));

    private static async Task<(HttpContext Context, string SeenPath)> RunAsync(
        HttpContext context, TenantTokenVerifier? verifier = null)
    {
        var seen = string.Empty;
        var middleware = new TenantContextMiddleware(
            ctx => { seen = ctx.Request.Path.Value ?? string.Empty; return Task.CompletedTask; },
            verifier);

        await middleware.InvokeAsync(context);
        return (context, seen);
    }

    private static HttpContext Request(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return context;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task LeavesAnOrdinaryRequestCompletelyUntouched()
    {
        // The acceptance bar for this milestone: today's traffic must be unaffected.
        var (context, seen) = await RunAsync(Request("/api/back-office/menus"));

        Assert.Equal("/api/back-office/menus", seen);
        Assert.Null(new TenantContextAccessor(Accessor(context)).Current);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task StripsTheTenantPrefixSoRouteTemplatesAreUnchanged()
    {
        // Decision 14.
        var (context, seen) = await RunAsync(Request($"/o/{Org}/v/{Venue}/api/back-office/menus"));

        Assert.Equal("/api/back-office/menus", seen);
        var resolved = new TenantContextAccessor(Accessor(context)).Current;
        Assert.Equal(Venue, resolved!.VenueId);
        Assert.Equal(TenantProvenance.Asserted, resolved.Provenance);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PrefersAValidTokenOverThePath()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var issuer = new TenantTokenIssuer(key, "k1", Clock);
        var tokenVenue = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var token = issuer.Issue(new TenantContext(tokenVenue, null, TenantProvenance.Verified), "1.5.0", TimeSpan.FromSeconds(60));

        var request = Request($"/o/{Org}/v/{Venue}/api/x");
        request.Request.Headers[TenantContextMiddleware.HeaderName] = token;

        var (context, _) = await RunAsync(request, new TenantTokenVerifier(key, "1.5.0", Clock));

        var resolved = new TenantContextAccessor(Accessor(context)).Current;
        Assert.Equal(tokenVenue, resolved!.VenueId);
        Assert.Equal(TenantProvenance.Verified, resolved.Provenance);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AnInvalidTokenResolvesNothingAndDoesNotFallBackToThePath()
    {
        // A token that fails verification is an attack or a bug, never an
        // invitation to trust the path instead.
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var stranger = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var token = new TenantTokenIssuer(stranger, "k1", Clock)
            .Issue(new TenantContext(Guid.NewGuid(), null, TenantProvenance.Verified), "1.5.0", TimeSpan.FromSeconds(60));

        var request = Request($"/o/{Org}/v/{Venue}/api/x");
        request.Request.Headers[TenantContextMiddleware.HeaderName] = token;

        var (context, seen) = await RunAsync(request, new TenantTokenVerifier(key, "1.5.0", Clock));

        Assert.Null(new TenantContextAccessor(Accessor(context)).Current);
        Assert.Equal("/api/x", seen);
    }

    private static IHttpContextAccessor Accessor(HttpContext context) =>
        new HttpContextAccessor { HttpContext = context };
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~TenantContextMiddlewareTests`
Expected: FAIL to compile — `The type or namespace name 'TenantContextMiddleware' could not be found`.

- [ ] **Step 4: Write the implementation**

Create `src/Vennu.Api/Infrastructure/TenantContextAccessor.cs`:

```csharp
using Vennu.Tenancy;

namespace Vennu.Api.Infrastructure;

/// <summary>
/// Reads the tenant the current request is about, if one was established.
/// Decision 11: this is a routing input. It must never reach an authorization decision —
/// authorization continues to derive from the session, the screen record, or the
/// Webhook Receiver's registration, exactly as it does today.
/// </summary>
public interface ITenantContextAccessor
{
    TenantContext? Current { get; }
}

public sealed class TenantContextAccessor(IHttpContextAccessor accessor) : ITenantContextAccessor
{
    public const string ItemKey = "Vennusign.TenantContext";

    public TenantContext? Current =>
        accessor.HttpContext?.Items.TryGetValue(ItemKey, out var value) == true
            ? value as TenantContext
            : null;
}
```

Create `src/Vennu.Api/Infrastructure/TenantContextMiddleware.cs`:

```csharp
using Vennu.Tenancy;

namespace Vennu.Api.Infrastructure;

/// <summary>
/// Establishes the tenant a request is about, from the internal token if the Product
/// Router minted one, otherwise from the URL prefix.
///
/// The path branch exists because the contract must work before the Router does
/// (decision 44). Once the Router is in front, it consumes the prefix itself
/// (decision 14) and only the token branch is exercised in production.
/// </summary>
public sealed class TenantContextMiddleware(RequestDelegate next, TenantTokenVerifier? verifier)
{
    public const string HeaderName = "X-Vennusign-Tenant-Token";

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (TenantPath.TryParse(path, out var fromPath, out var remainder))
        {
            context.Request.Path = remainder;
        }

        if (context.Request.Headers.TryGetValue(HeaderName, out var header) &&
            header.Count > 0 && verifier is not null)
        {
            // A token that fails verification resolves nothing. It is never a reason
            // to fall back to the caller-supplied path.
            if (verifier.TryVerify(header[0]!, out var fromToken, out _))
            {
                context.Items[TenantContextAccessor.ItemKey] = fromToken;
            }
        }
        else if (fromPath is not null)
        {
            context.Items[TenantContextAccessor.ItemKey] = fromPath;
        }

        await next(context).ConfigureAwait(false);
    }
}
```

- [ ] **Step 5: Run the tests to verify they pass**

Run: `dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~TenantContextMiddlewareTests`
Expected: PASS, 4 of 4.

- [ ] **Step 6: Commit**

```bash
git add src/Vennu.Api/Infrastructure/TenantContextAccessor.cs \
        src/Vennu.Api/Infrastructure/TenantContextMiddleware.cs \
        src/Vennu.Api/Vennu.Api.csproj \
        tests/Vennu.Api.Tests/Infrastructure/TenantContextMiddlewareTests.cs \
        tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj
git commit -m "feat(api): resolve TenantContext from token or path prefix

Token wins where present; an invalid one resolves nothing rather than
falling back to the caller-supplied path. The prefix is stripped so route
templates are unchanged, per decision 14."
```

---

### Task 5: Wire it into the request pipeline

**Files:**
- Modify: `src/Vennu.Api/Program.cs:310` (immediately after `app.UseMiddleware<AdministrativeCompatibilityMiddleware>()`)
- Create: `tests/Vennu.Api.Tests/Infrastructure/TenantContextPipelineTests.cs`

**Interfaces:**
- Consumes: everything from Tasks 1–4.
- Produces: nothing. This task makes the milestone live and proves it changed nothing.

The middleware is registered **before** `UseAuthentication`, because the prefix must be stripped before routing and authentication run. `TenantTokenVerifier` is registered as `null` for now — no Router exists to mint tokens, and the public key's home is a later milestone (register Q17). The path branch is what functions in this milestone.

- [ ] **Step 1: Write the failing test**

Create `tests/Vennu.Api.Tests/Infrastructure/TenantContextPipelineTests.cs`:

```csharp
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Vennu.Api.Tests.Infrastructure;

public sealed class TenantContextPipelineTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task HealthVersionStillAnswersWithoutAPrefix()
    {
        var response = await factory.CreateClient().GetAsync("/health/version");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task HealthVersionAnswersThroughATenantPrefix()
    {
        // Decision 14: the prefix is consumed before routing, so an existing
        // endpoint answers identically whether or not one is present.
        var org = Guid.NewGuid();
        var venue = Guid.NewGuid();

        var response = await factory.CreateClient().GetAsync($"/o/{org}/v/{venue}/health/version");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task AMalformedPrefixIsNotTreatedAsOne()
    {
        var response = await factory.CreateClient().GetAsync("/o/not-a-guid/v/also-not/health/version");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~TenantContextPipelineTests`
Expected: `HealthVersionAnswersThroughATenantPrefix` FAILS with `NotFound` — the prefix is not yet stripped, so nothing matches the route.

- [ ] **Step 3: Register the services and the middleware**

In `src/Vennu.Api/Program.cs`, alongside the other `builder.Services` registrations (near line 188, where `AddSignalR` sits), add:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<Vennu.Api.Infrastructure.ITenantContextAccessor,
                              Vennu.Api.Infrastructure.TenantContextAccessor>();
```

Then, immediately after the existing `app.UseMiddleware<AdministrativeCompatibilityMiddleware>();` on line 310, add:

```csharp
// Keystone milestone 1. The verifier is null until the Product Router exists to mint
// tokens and the public key has a home (register Q17); until then the path branch
// is what functions, and a request carrying neither is untouched.
app.UseMiddleware<TenantContextMiddleware>((Vennu.Tenancy.TenantTokenVerifier?)null);
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj --filter FullyQualifiedName~TenantContextPipelineTests`
Expected: PASS, 3 of 3.

- [ ] **Step 5: Run the full affected suites**

```bash
dotnet build src/Vennu.Api/Vennu.Api.csproj -c Release
dotnet test tests/Vennu.Tenancy.Tests/Vennu.Tenancy.Tests.csproj
dotnet test tests/Vennu.Api.Tests/Vennu.Api.Tests.csproj
```

Expected: Release build succeeds. `Vennu.Tenancy.Tests` 23 of 23. `Vennu.Api.Tests` passes with no regressions — record the before-and-after counts in the PR, per AGENTS.md's evidence rule. Azure and integration-type tests remain skipped by standing owner exception; record them as skipped.

- [ ] **Step 6: Commit**

```bash
git add src/Vennu.Api/Program.cs tests/Vennu.Api.Tests/Infrastructure/TenantContextPipelineTests.cs
git commit -m "feat(api): register TenantContext resolution in the pipeline

Runs before authentication so the prefix is stripped before routing. The
token verifier is null until a Router exists to mint tokens. A request
carrying neither prefix nor token is untouched, which is this milestone's
acceptance bar."
```

---

## What this milestone deliberately excludes

- **The front-end URL restructure.** Decision 44 places it in milestone 1, but register **Q31 is deferred**: whether the pre-auth app split lands before the restructure is unanswered, and doing them in the wrong order means doing the entry routes twice. The .NET half above is independent of that answer and ships without it.
- **Relative API URLs in the front ends** (register Q20) — same dependency.
- **The 421 misdirected-request behaviour** (decision 35). It needs an authority to compare a hint against, and no consumer reads `TenantContext` yet. It belongs with the first consumer.
- **Key management** (register Q17). No Router exists to hold a private key, so the verifier is null.
- **VDS, ADS, the Router itself** — milestone 4 onward.

## Self-review

**Spec coverage.** Decisions 10, 11, 13, 14, 18, 32, 33 and 34 each have a task and at least one test asserting them. Decisions 1–9, 12, 15–17, 19–31, 35–49 are out of this milestone's scope and are named above where a reader might expect them. Register answers Q15 (ES256), Q16 (60s TTL) and Q17 (key location deferred) are reflected.

**Placeholders.** None. Every code step carries the actual code; every run step carries the exact command and expected result.

**Type consistency.** `TenantContext(Guid, Guid?, TenantProvenance)` is constructed identically in Tasks 1–5. `TryParse(string, out TenantContext?, out string)` and `TryVerify(string, out TenantContext?, out string?)` keep the same shapes throughout. `TenantContextAccessor.ItemKey` is the single place the `HttpContext.Items` key is spelled.

**Known gap, stated rather than hidden.** Task 5's `AMalformedPrefixIsNotTreatedAsOne` asserts `NotFound`, which is what an unmatched route returns today. If a future catch-all route changes that, this assertion is testing the router rather than the middleware. It is kept because it is the cheapest guard against `TenantPath.TryParse` becoming accidentally permissive.
