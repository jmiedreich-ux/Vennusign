using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.BackOffice;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireCapabilityAttribute : TypeFilterAttribute
{
    public RequireCapabilityAttribute(string capabilityId) : base(typeof(CapabilityActionAuthorizationFilter)) =>
        Arguments = [capabilityId];
}

public sealed class CapabilityActionAuthorizationFilter(
    string capabilityId,
    ICapabilityActionAuthorizer authorizer,
    ICapabilityMessageCatalog messages) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var locale = context.HttpContext.Request.GetTypedHeaders().AcceptLanguage?.FirstOrDefault()?.Value.Value ?? "en-US";
        try
        {
            await authorizer.RequireAllowedAsync(
                CapabilityId.Parse(capabilityId),
                context.HttpContext.TraceIdentifier,
                locale,
                context.HttpContext.RequestAborted).ConfigureAwait(false);
            await next().ConfigureAwait(false);
        }
        catch (CapabilityDecisionDeniedException exception)
        {
            var decision = exception.Decision;
            context.Result = new ObjectResult(new
            {
                capabilityId = decision.Capability.Value,
                decision = ToApiValue(decision.Decision),
                decision.ReasonCode,
                category = ToApiValue(decision.Category),
                message = messages.Resolve(decision.Locale, decision.MessageKey, decision.Parameters),
                decision.Resolution,
                retryAfterSeconds = decision.RetryAfter is TimeSpan retry ? (int)Math.Ceiling(retry.TotalSeconds) : (int?)null,
                decision.CorrelationId
            }) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }

    private static string ToApiValue<T>(T value) where T : Enum =>
        System.Text.RegularExpressions.Regex.Replace(value.ToString(), "([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
}
