namespace Vennu.Api.CustomerAuthentication;

public static class CustomerSessionCookie
{
    public static bool TryRead(HttpRequest request, out string token)
    {
        if (request.Cookies.TryGetValue(CustomerAuthenticationDefaults.SessionCookieName, out token!) &&
            !string.IsNullOrWhiteSpace(token))
            return true;
        return request.Cookies.TryGetValue(CustomerAuthenticationDefaults.LegacySessionCookieName, out token!) &&
            !string.IsNullOrWhiteSpace(token);
    }

    public static void Append(HttpResponse response, string token, DateTime expiresUtc) =>
        response.Cookies.Append(
            CustomerAuthenticationDefaults.SessionCookieName,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = new DateTimeOffset(expiresUtc),
                IsEssential = true
            });

    public static void Delete(HttpResponse response)
    {
        response.Cookies.Delete(
            CustomerAuthenticationDefaults.SessionCookieName,
            new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/" });
        response.Cookies.Delete(
            CustomerAuthenticationDefaults.LegacySessionCookieName,
            new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/" });
    }
}
