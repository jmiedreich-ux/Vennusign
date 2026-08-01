$repoRoot = Split-Path -Parent $PSScriptRoot

$apiBaseUrl = 'http://localhost:5192'
$venueAdminBaseUrl = 'http://localhost:5174/venue-admin/'
$displayBaseUrl = 'http://localhost:5175'

function Start-DevShell {
    param(
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,
        [Parameter(Mandatory = $true)]
        [string[]]$Commands
    )

    $script = @(
        "Set-Location '$WorkingDirectory'"
        $Commands
    ) -join [Environment]::NewLine

    Start-Process powershell -ArgumentList '-NoExit', '-Command', $script
}

Start-DevShell -WorkingDirectory $repoRoot -Commands @(
    "`$env:ASPNETCORE_ENVIRONMENT = 'Development'"
    'dotnet run --no-build --launch-profile http --project .\src\Vennu.Api\Vennu.Api.csproj'
)

Start-DevShell -WorkingDirectory "$repoRoot\src\admin" -Commands @(
    "`$env:VITE_VENNU_API_BASE_URL = '$apiBaseUrl'"
    "`$env:VITE_VENNU_DISPLAY_BASE_URL = '$displayBaseUrl'"
    "`$env:VITE_VENNU_VENUE_ADMIN_BASE_URL = '$venueAdminBaseUrl'"
    'npm run dev -- --host localhost --port 5173'
)

Start-DevShell -WorkingDirectory "$repoRoot\src\venue-admin" -Commands @(
    "`$env:VITE_VENNU_API_BASE_URL = '$apiBaseUrl'"
    'npm run dev -- --host localhost --port 5174'
)

Start-DevShell -WorkingDirectory "$repoRoot\src\display" -Commands @(
    "`$env:VITE_API_BASE_URL = '$apiBaseUrl'"
    "`$env:VITE_SIGNALR_HUB_URL = '$apiBaseUrl/hubs/vennu'"
    'npm run dev -- --host localhost --port 5175'
)
