namespace Vennu.Api.Infrastructure;

public sealed class AdministrativeCompatibilityMiddleware(
    RequestDelegate next,
    ILogger<AdministrativeCompatibilityMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var contract = LegacyContract(context.Request);
        if (contract is not null)
        {
            logger.LogInformation("Legacy administrative contract used: {Contract}", contract);
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["Deprecation"] = "true";
                return Task.CompletedTask;
            });
        }

        await next(context).ConfigureAwait(false);
    }

    private static string? LegacyContract(HttpRequest request)
    {
        if (request.Path.StartsWithSegments("/api/admin")) return "platform-operations-route";
        if (request.Path.StartsWithSegments("/api/venue-admin")) return "back-office-route";
        if (request.Path.StartsWithSegments("/hubs/vennu")) return "signalr-route";
        if (request.Headers.ContainsKey("X-Vennu-Admin-Key")) return "platform-operations-header";
        if (request.Headers.ContainsKey("X-Vennu-Venue-Token") || request.Headers.ContainsKey("X-Vennu-Venue-Id"))
            return "back-office-header";
        if (request.Cookies.ContainsKey("__Host-Vennu.CustomerSession")) return "customer-session-cookie";
        return null;
    }
}
