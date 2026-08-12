using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Vennu.TestApi;

public sealed class TestApiAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IOptions<TestApiOptions> options)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var supplied = context.Request.Headers["X-Vennusign-Test-Api-Key"].ToString();
        var configured = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(supplied) || !KeysMatch(supplied, configured))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next(context).ConfigureAwait(false);
    }

    private static bool KeysMatch(string supplied, string configured) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(supplied)),
            SHA256.HashData(Encoding.UTF8.GetBytes(configured)));
}
