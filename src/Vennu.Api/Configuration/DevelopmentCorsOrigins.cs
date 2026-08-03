namespace Vennu.Api.Configuration;

public static class DevelopmentCorsOrigins
{
    public static string[] Values { get; } =
    [
        "http://localhost:5173",
        "https://localhost:5173",
        "http://localhost:5174",
        "https://localhost:5174",
        "http://localhost:5175",
        "https://localhost:5175"
    ];
}
