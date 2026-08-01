$repoRoot = Split-Path -Parent $PSScriptRoot

$apiBaseUrl = 'http://localhost:5192'
$venueAdminBaseUrl = 'http://localhost:5174/venue-admin/'
$displayBaseUrl = 'http://localhost:5175'

Start-Process powershell -ArgumentList '-NoExit', '-Command', "Set-Location '$repoRoot'; dotnet run --no-build --launch-profile http --project .\src\Vennu.Api\Vennu.Api.csproj"

Start-Process powershell -ArgumentList '-NoExit', '-Command', "Set-Location '$repoRoot\src\admin'; `$env:VITE_VENNU_API_BASE_URL='$apiBaseUrl'; `$env:VITE_VENNU_DISPLAY_BASE_URL='$displayBaseUrl'; `$env:VITE_VENNU_VENUE_ADMIN_BASE_URL='$venueAdminBaseUrl'; npm run dev -- --host 127.0.0.1 --port 5173"

Start-Process powershell -ArgumentList '-NoExit', '-Command', "Set-Location '$repoRoot\src\venue-admin'; `$env:VITE_VENNU_API_BASE_URL='$apiBaseUrl'; npm run dev -- --host 127.0.0.1 --port 5174"

Start-Process powershell -ArgumentList '-NoExit', '-Command', "Set-Location '$repoRoot\src\display'; `$env:VITE_API_BASE_URL='$apiBaseUrl'; `$env:VITE_SIGNALR_HUB_URL='$apiBaseUrl/hubs/vennu'; npm run dev -- --host 127.0.0.1 --port 5175"
