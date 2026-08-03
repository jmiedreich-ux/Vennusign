namespace Vennu.Api.CustomerAuthentication;

public static class CustomerReturnUri
{
    public static bool TryCreate(Uri frontendOrigin, string returnPath, out Uri returnUri)
    {
        returnUri = null!;
        if (!IsValidOrigin(frontendOrigin) || !IsLocalPath(returnPath)) return false;
        returnUri = new Uri(frontendOrigin, returnPath);
        return true;
    }

    public static bool IsValidOrigin(Uri? origin) =>
        origin is { IsAbsoluteUri: true } &&
        origin.Scheme == Uri.UriSchemeHttps &&
        string.IsNullOrEmpty(origin.UserInfo) &&
        origin.AbsolutePath == "/" &&
        string.IsNullOrEmpty(origin.Query) &&
        string.IsNullOrEmpty(origin.Fragment);

    public static bool IsLocalPath(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        path.StartsWith("/", StringComparison.Ordinal) &&
        !path.StartsWith("//", StringComparison.Ordinal) &&
        !path.StartsWith("/\\", StringComparison.Ordinal) &&
        path.Length <= 500;
}
