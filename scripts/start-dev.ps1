$repoRoot = Split-Path -Parent $PSScriptRoot

$apiBaseUrl = 'http://localhost:5192'
$backOfficeBaseUrl = 'http://localhost:5174/'
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

Start-DevShell -WorkingDirectory "$repoRoot\src\platform-operations" -Commands @(
    "`$env:VITE_API_URL = '$apiBaseUrl'"
    "`$env:VITE_DISPLAY_URL = '$displayBaseUrl'"
    "`$env:VITE_BACK_OFFICE_URL = '$backOfficeBaseUrl'"
    'npm run dev -- --host localhost --port 5173'
)

Start-DevShell -WorkingDirectory "$repoRoot\src\back-office" -Commands @(
    "`$env:VITE_API_URL = '$apiBaseUrl'"
    'npm run dev -- --host localhost --port 5174'
)

Start-DevShell -WorkingDirectory "$repoRoot\src\display" -Commands @(
    "`$env:VITE_API_URL = '$apiBaseUrl'"
    "`$env:VITE_SIGNALR_URL = '$apiBaseUrl/hubs/vennusign'"
    'npm run dev -- --host localhost --port 5175'
)
