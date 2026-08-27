namespace Vennu.TestApi;

/// <summary>
/// Whether this outbound call may accept a certificate the machine does not trust.
///
/// The UI suite failed entirely, on every branch, for months. Not one assertion ran: every seed
/// returned 500 with `AuthenticationException: ... UntrustedRoot`, because this service calls the
/// product API at https://localhost:7138 and a .NET HttpClient validates the chain. The ASP.NET
/// Core development certificate is self-signed, so on a runner that has never trusted it the chain
/// ends in an untrusted root. Playwright was never the problem - it sets ignoreHTTPSErrors, which
/// covers the browser and not HttpClient.
///
/// The obvious fix is `dotnet dev-certs https --trust` in CI, and it was tried (#866). It hangs:
/// adding to the Windows Root store raises a confirmation dialog, and a headless runner has nobody
/// to click it. Machine trust is the wrong thing to depend on anyway - it makes a green suite a
/// property of how a machine was once set up rather than of the code.
///
/// So this decides it in the process, and it is deliberately hard to widen:
///
/// - it is OFF unless configuration turns it on, so nothing is inherited from an environment name
///   or a build flag that could drift
/// - it applies to LOOPBACK ONLY. A configuration mistake pointing this service at a real host
///   cannot become "accept any certificate from anyone", which is what a bare
///   DangerousAcceptAnyServerCertificateValidator would have meant
///
/// Both conditions must hold. Either alone would be a hole.
/// </summary>
public static class LoopbackCertificateTrust
{
    /// <summary>
    /// True only for a request this process may make against a self-signed certificate.
    ///
    /// <paramref name="requestUri"/> is the request's own URI rather than the client's base
    /// address, so a redirect away from localhost is judged on where it actually went.
    /// </summary>
    public static bool Allows(bool configured, Uri? requestUri)
    {
        if (!configured || requestUri is null) return false;
        if (!requestUri.IsAbsoluteUri) return false;

        // IsLoopback covers 127.0.0.0/8, ::1 and the literal "localhost". A host that merely
        // resolves to a loopback address does not count: that would let DNS decide what this
        // process trusts.
        return requestUri.IsLoopback;
    }

    /// <summary>
    /// The handler the seeding client actually uses.
    ///
    /// It exists as a function so a test can exercise the real thing against a real self-signed
    /// server. A test that rebuilt an equivalent handler of its own would prove that the test knows
    /// how to write one, which is not the thing in doubt.
    /// </summary>
    public static HttpClientHandler CreateHandler(bool configured) => new()
    {
        ServerCertificateCustomValidationCallback = (request, _, _, errors) =>
            errors == System.Net.Security.SslPolicyErrors.None
            || Allows(configured, request.RequestUri)
    };
}
