using System.IO;

namespace Vennu.DevControl;

public static class DevelopmentServiceCatalog
{
    public static IReadOnlyList<ServiceEntry> Create(string repositoryRoot) =>
    [
        new("API", 7138, repositoryRoot, "dotnet", "run --no-build --launch-profile https --project .\\src\\Vennu.Api\\Vennu.Api.csproj", "https://localhost:7138", ["ASPNETCORE_ENVIRONMENT=Development"]),
        new("Admin", 5173, Path.Combine(repositoryRoot, "src", "admin"), "cmd.exe", "/c npm run dev -- --host localhost --port 5173", "http://localhost:5173", ["VITE_VENNU_API_BASE_URL=https://localhost:7138", "VITE_VENNU_DISPLAY_BASE_URL=http://localhost:5175", "VITE_VENNU_VENUE_ADMIN_BASE_URL=https://localhost:5174/"]),
        new("Venue Admin", 5174, Path.Combine(repositoryRoot, "src", "venue-admin"), "cmd.exe", "/c npm run dev -- --host localhost --port 5174", "https://localhost:5174/", ["VITE_VENNU_API_BASE_URL=https://localhost:7138"]),
        new("Display", 5175, Path.Combine(repositoryRoot, "src", "display"), "cmd.exe", "/c npm run dev -- --host localhost --port 5175", "http://localhost:5175", ["VITE_API_BASE_URL=https://localhost:7138", "VITE_SIGNALR_HUB_URL=https://localhost:7138/hubs/vennu"])
    ];
}
