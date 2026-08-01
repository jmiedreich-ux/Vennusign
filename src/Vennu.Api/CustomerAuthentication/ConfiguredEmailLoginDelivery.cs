using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Vennu.Data.Services;

namespace Vennu.Api.CustomerAuthentication;

public sealed class ConfiguredEmailLoginDelivery(
    HttpClient httpClient,
    IOptions<CustomerAuthenticationOptions> options,
    ILogger<ConfiguredEmailLoginDelivery> logger) : IEmailLoginDelivery
{
    public async Task SendAsync(EmailLoginDelivery delivery, CancellationToken cancellationToken = default)
    {
        var configuration = options.Value.EmailDelivery;
        if (!configuration.Enabled || configuration.Endpoint is null)
        {
            logger.LogWarning("Customer email-link delivery is disabled; no link was transmitted.");
            return;
        }
        using var request = new HttpRequestMessage(HttpMethod.Post, configuration.Endpoint)
        {
            Content = JsonContent.Create(new
            {
                delivery.Email,
                delivery.Token,
                delivery.ReturnPath,
                delivery.ExpiresUtc
            })
        };
        if (!string.IsNullOrWhiteSpace(configuration.ApiKey))
            request.Headers.TryAddWithoutValidation("X-Api-Key", configuration.ApiKey);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
