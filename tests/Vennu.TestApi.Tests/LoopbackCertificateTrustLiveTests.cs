using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vennu.TestApi;
using Xunit;

namespace Vennu.TestApi.Tests;

/// <summary>
/// The fix, against a real self-signed certificate over a real socket.
///
/// The predicate tests next door check the RULE. These check the WIRING, because the defect they
/// exist for was never a wrong rule - it was an HttpClient that validated a chain nobody had
/// trusted, and every unit test in the repository passed while the entire UI suite could not seed.
///
/// A test that built its own handler would prove only that the test knows how to write one, so
/// these call <see cref="LoopbackCertificateTrust.CreateHandler"/> - the same function Program.cs
/// registers.
/// </summary>
[Trait("Category", "Unit")]
public sealed class LoopbackCertificateTrustLiveTests : IAsyncLifetime
{
    private IHost? host;
    private string origin = "";

    public async Task InitializeAsync()
    {
        // A certificate no machine has ever trusted, made here so the test does not depend on
        // whether `dotnet dev-certs` was ever run - which is the whole thing being removed.
        using var key = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(BuildSan().Build());
        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var exportable = X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pfx), null);

        host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web
                .UseKestrel(server => server.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(exportable)))
                .Configure(app => app.Run(context => context.Response.WriteAsync("seeded"))))
            .Build();

        await host.StartAsync();
        origin = host.Services.GetRequiredService<IServer>().Features
            .Get<IServerAddressesFeature>()!.Addresses.First();
    }

    public async Task DisposeAsync()
    {
        if (host is not null) { await host.StopAsync(); host.Dispose(); }
    }

    [Fact]
    public async Task WithTheSettingOff_TheCallFailsExactlyAsCiDid()
    {
        // This is the failure the whole UI suite died on: an untrusted root, surfacing as a 500
        // from every seed. Asserting it here means the fix below is measured against the real
        // thing rather than against nothing.
        using var client = new HttpClient(LoopbackCertificateTrust.CreateHandler(configured: false));

        var failure = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync(origin));
        Assert.IsAssignableFrom<System.Security.Authentication.AuthenticationException>(failure.InnerException);
    }

    [Fact]
    public async Task WithTheSettingOn_TheCallSucceedsOnLoopback()
    {
        using var client = new HttpClient(LoopbackCertificateTrust.CreateHandler(configured: true));

        Assert.Equal("seeded", await client.GetStringAsync(origin));
    }

    private static SubjectAlternativeNameBuilder BuildSan()
    {
        var builder = new SubjectAlternativeNameBuilder();
        builder.AddDnsName("localhost");
        builder.AddIpAddress(IPAddress.Loopback);
        return builder;
    }
}
