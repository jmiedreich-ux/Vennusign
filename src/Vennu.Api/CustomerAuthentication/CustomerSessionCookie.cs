namespace Vennu.Api.CustomerAuthentication;

public static class CustomerSessionCookie
{
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

    public static void Delete(HttpResponse response) =>
        response.Cookies.Delete(
            CustomerAuthenticationDefaults.SessionCookieName,
            new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/" });
}
