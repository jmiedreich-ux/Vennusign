using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Vennu.Api.Pos;

public sealed class CloverOAuthGateway(HttpClient httpClient, IOptions<CloverOAuthOptions> options)
    : ICloverOAuthGateway
{
    public Uri CreateAuthorizationUri(string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        var value = RequireOptions();
        return new Uri(QueryHelpers.AddQueryString(value.AuthorizationEndpoint, new Dictionary<string, string?>
        {
            ["client_id"] = value.ClientId,
            ["redirect_uri"] = RequireHttps(value.CallbackUrl, "callback").AbsoluteUri,
            ["state"] = state
        }));
    }

    public async Task<CloverOAuthTokenResult> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var value = RequireOptions();
        using var response = await httpClient.PostAsJsonAsync(
            RequireTokenEndpoint(value.TokenEndpoint),
            new Dictionary<string, string>
            {
                ["client_id"] = value.ClientId,
                ["client_secret"] = value.ClientSecret,
                ["code"] = code
            }, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Clover returned an empty OAuth response.");
        if (string.IsNullOrWhiteSpace(token.AccessToken) || string.IsNullOrWhiteSpace(token.RefreshToken))
            throw new InvalidOperationException("Clover returned incomplete OAuth credentials.");
        return new CloverOAuthTokenResult(
            token.AccessToken,
            token.RefreshToken,
            FromUnixSeconds(token.AccessTokenExpiration, "access token"),
            FromUnixSeconds(token.RefreshTokenExpiration, "refresh token"));
    }

    public void ValidateClientId(string clientId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        if (!string.Equals(clientId, RequireOptions().ClientId, StringComparison.Ordinal))
            throw new InvalidOperationException("The Clover OAuth callback client does not match this application.");
    }

    public Uri CreateReturnUri(string outcome)
    {
        var value = RequireOptions();
        return new Uri(QueryHelpers.AddQueryString(
            RequireHttps(value.BackOfficeReturnUrl, "Back Office return").AbsoluteUri,
            "pos",
            outcome));
    }

    private CloverOAuthOptions RequireOptions()
    {
        var value = options.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ClientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(value.ClientSecret);
        RequireAuthorizationEndpoint(value.AuthorizationEndpoint);
        RequireTokenEndpoint(value.TokenEndpoint);
        return value;
    }

    private static Uri RequireAuthorizationEndpoint(string value)
    {
        var uri = RequireHttps(value, "authorization");
        if (uri.Host is not ("sandbox.dev.clover.com" or "www.clover.com" or "www.eu.clover.com" or "www.la.clover.com") ||
            uri.AbsolutePath != "/oauth/v2/authorize")
            throw new InvalidOperationException("Clover OAuth authorization endpoint is not allowlisted.");
        return uri;
    }

    private static Uri RequireTokenEndpoint(string value)
    {
        var uri = RequireHttps(value, "token");
        if (uri.Host is not ("apisandbox.dev.clover.com" or "api.clover.com" or "api.eu.clover.com" or "api.la.clover.com") ||
            uri.AbsolutePath != "/oauth/v2/token")
            throw new InvalidOperationException("Clover OAuth token endpoint is not allowlisted.");
        return uri;
    }

    private static Uri RequireHttps(string value, string name) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? uri
            : throw new InvalidOperationException($"Clover OAuth {name} URL must be absolute HTTPS.");

    private static DateTime FromUnixSeconds(long value, string name)
    {
        try { return DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime; }
        catch (ArgumentOutOfRangeException exception)
        { throw new InvalidOperationException($"Clover returned an invalid {name} expiration.", exception); }
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("access_token_expiration")] long AccessTokenExpiration,
        [property: JsonPropertyName("refresh_token_expiration")] long RefreshTokenExpiration);
}
