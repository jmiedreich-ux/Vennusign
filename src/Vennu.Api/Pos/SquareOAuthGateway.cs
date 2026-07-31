using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Vennu.Api.Pos;

public sealed class SquareOAuthGateway(HttpClient httpClient, IOptions<SquareOAuthOptions> options)
    : ISquareOAuthGateway
{
    public Uri CreateAuthorizationUri(string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        var value = RequireOptions();
        return new Uri(QueryHelpers.AddQueryString(value.AuthorizationEndpoint, new Dictionary<string, string?>
        {
            ["client_id"] = value.ApplicationId,
            ["scope"] = string.Join(' ', value.Scopes),
            ["state"] = state,
            ["redirect_uri"] = RequireHttps(value.CallbackUrl, "callback").AbsoluteUri,
            ["session"] = "false"
        }));
    }

    public async Task<SquareOAuthTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var value = RequireOptions();
        using var response = await httpClient.PostAsJsonAsync(
            value.TokenEndpoint,
            new Dictionary<string, string>
            {
                ["client_id"] = value.ApplicationId,
                ["client_secret"] = value.ApplicationSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = RequireHttps(value.CallbackUrl, "callback").AbsoluteUri
            }, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Square returned an empty OAuth response.");
        return new SquareOAuthTokenResult(token.MerchantId, token.AccessToken, token.RefreshToken, token.ExpiresAt?.UtcDateTime);
    }

    public async Task RevokeAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        var value = RequireOptions();
        using var request = new HttpRequestMessage(HttpMethod.Post, value.RevokeEndpoint)
        {
            Content = JsonContent.Create(new { client_id = value.ApplicationId, access_token = accessToken })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Client", value.ApplicationSecret);
        request.Headers.Add("Square-Version", value.ApiVersion);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public Uri CreateReturnUri(string outcome)
    {
        var value = RequireOptions();
        return new Uri(QueryHelpers.AddQueryString(
            RequireHttps(value.VenueAdminReturnUrl, "Venue Admin return").AbsoluteUri,
            "pos",
            outcome));
    }

    private SquareOAuthOptions RequireOptions()
    {
        var value = options.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ApplicationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ApplicationSecret);
        RequireSquareEndpoint(value.AuthorizationEndpoint, "authorization");
        RequireSquareEndpoint(value.TokenEndpoint, "token");
        RequireSquareEndpoint(value.RevokeEndpoint, "revoke");
        return value;
    }

    private static Uri RequireSquareEndpoint(string value, string name)
    {
        var uri = RequireHttps(value, name);
        if (uri.Host is not ("connect.squareup.com" or "connect.squareupsandbox.com"))
            throw new InvalidOperationException($"Square OAuth {name} endpoint is not allowlisted.");
        return uri;
    }

    private static Uri RequireHttps(string value, string name) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? uri
            : throw new InvalidOperationException($"Square OAuth {name} URL must be absolute HTTPS.");

    private sealed record TokenResponse(
        [property: JsonPropertyName("merchant_id")] string MerchantId,
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("expires_at")] DateTimeOffset? ExpiresAt);
}
