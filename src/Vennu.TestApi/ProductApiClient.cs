using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Vennu.TestApi;

public sealed class ProductApiClient(HttpClient httpClient, IOptions<TestApiOptions> options)
{
    public async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        string accessToken,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Vennusign-Back-Office-Token", accessToken);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return (await response.Content.ReadFromJsonAsync<T>(cancellationToken).ConfigureAwait(false))
            ?? throw new InvalidOperationException($"The product API returned an empty response for {path}.");
    }

    public async Task<T> SendPublicAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return (await response.Content.ReadFromJsonAsync<T>(cancellationToken).ConfigureAwait(false))
            ?? throw new InvalidOperationException($"The product API returned an empty response for {path}.");
    }

    public async Task SendPublicAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendAsync(
        HttpMethod method,
        string path,
        string accessToken,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Vennusign-Back-Office-Token", accessToken);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendAutomationAsync(string path, object body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("X-Vennusign-Test-Automation-Key", options.Value.ProductAutomationKey);
        request.Content = JsonContent.Create(body);
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var detail = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new ProductApiException((int)response.StatusCode, detail);
    }
}

public sealed class ProductApiException(int statusCode, string detail) : Exception(detail)
{
    public int StatusCode { get; } = statusCode;
}
