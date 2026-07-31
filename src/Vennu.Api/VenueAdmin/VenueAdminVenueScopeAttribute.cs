using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Vennu.Api.VenueAdmin;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class VenueAdminVenueScopeAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var venueClaim = context.HttpContext.User.FindFirstValue(
            VenueAdminAuthenticationDefaults.VenueIdClaim);
        var hasRouteVenue = TryResolveRouteVenue(context, out var routeVenueId);

        if (!hasRouteVenue ||
            !Guid.TryParse(venueClaim, out var authorizedVenueId) ||
            authorizedVenueId != routeVenueId)
        {
            context.Result = new ForbidResult(
                VenueAdminAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        await next().ConfigureAwait(false);
    }

    private static bool TryResolveRouteVenue(
        ActionExecutingContext context,
        out Guid venueId)
    {
        if (context.ActionArguments.TryGetValue("venueId", out var argument) &&
            argument is Guid argumentVenueId)
        {
            venueId = argumentVenueId;
            return true;
        }

        return Guid.TryParse(
            context.RouteData.Values["venueId"]?.ToString(),
            out venueId);
    }
}
