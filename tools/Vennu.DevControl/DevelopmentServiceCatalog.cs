using System.IO;

namespace Vennu.DevControl;

public static class DevelopmentServiceCatalog
{
    public static IReadOnlyList<ServiceEntry> Create(string repositoryRoot) =>
    [
        new("API", 7138, repositoryRoot, "dotnet", "run --no-build --launch-profile https --project .\\src\\Vennu.Api\\Vennu.Api.csproj", "https://localhost:7138", ["ASPNETCORE_ENVIRONMENT=Development"]),
        new("Platform Operations", 5173, Path.Combine(repositoryRoot, "src", "platform-operations"), "cmd.exe", "/c npm run dev -- --host localhost --port 5173", "http://localhost:5173", ["VITE_API_URL=https://localhost:7138", "VITE_DISPLAY_URL=http://localhost:5175", "VITE_BACK_OFFICE_URL=https://localhost:5174/"]),
        new("Back Office", 5174, Path.Combine(repositoryRoot, "src", "back-office"), "cmd.exe", "/c npm run dev -- --host localhost --port 5174", "https://localhost:5174/", ["VITE_API_URL=https://localhost:7138"]),
        new("Display", 5175, Path.Combine(repositoryRoot, "src", "display"), "cmd.exe", "/c npm run dev -- --host localhost --port 5175", "http://localhost:5175", ["VITE_API_URL=https://localhost:7138", "VITE_SIGNALR_URL=https://localhost:7138/hubs/vennusign"])
    ];
}
