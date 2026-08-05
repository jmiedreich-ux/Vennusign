$repoRoot = Split-Path -Parent $PSScriptRoot

$apiBaseUrl = 'https://localhost:7138'
$backOfficeBaseUrl = 'https://localhost:5174'
$displayBaseUrl = 'http://localhost:5175'

$organizationId = '72000000-0000-0000-0000-000000000001'
$venueId = '73000000-0000-0000-0000-000000000001'

function Start-AcceptanceShell {
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

$sessions = @(
    @{ Token = 'track1-owner-review'; UserId = '71000000-0000-0000-0000-000000000001'; Name = 'Track 1 Owner Review'; Role = 'organization_owner' },
    @{ Token = 'track1-content-editor'; UserId = '71000000-0000-0000-0000-000000000002'; Name = 'Track 1 Content Editor'; Role = 'content_editor' },
    @{ Token = 'track1-publisher'; UserId = '71000000-0000-0000-0000-000000000003'; Name = 'Track 1 Publisher'; Role = 'publisher' }
)

$apiCommands = @("`$env:ASPNETCORE_ENVIRONMENT = 'Development'")
for ($index = 0; $index -lt $sessions.Count; $index++) {
    $session = $sessions[$index]
    $apiCommands += "`$env:BackOffice__Sessions__${index}__AccessToken = '$($session.Token)'"
    $apiCommands += "`$env:BackOffice__Sessions__${index}__VenueId = '$venueId'"
    $apiCommands += "`$env:BackOffice__Sessions__${index}__OrganizationId = '$organizationId'"
    $apiCommands += "`$env:BackOffice__Sessions__${index}__UserId = '$($session.UserId)'"
    $apiCommands += "`$env:BackOffice__Sessions__${index}__DisplayName = '$($session.Name)'"
    $apiCommands += "`$env:BackOffice__Sessions__${index}__SystemRole = '$($session.Role)'"
}
$apiCommands += 'dotnet run --launch-profile https --project .\src\Vennu.Api\Vennu.Api.csproj'

Start-AcceptanceShell -WorkingDirectory $repoRoot -Commands $apiCommands

Start-AcceptanceShell -WorkingDirectory "$repoRoot\src\back-office" -Commands @(
    "`$env:VITE_VENNUSIGN_API_BASE_URL = '$apiBaseUrl'"
    "`$env:VITE_VENNUSIGN_DISPLAY_BASE_URL = '$displayBaseUrl'"
    'npm run dev -- --host localhost --port 5174'
)

Start-AcceptanceShell -WorkingDirectory "$repoRoot\src\display" -Commands @(
    "`$env:VITE_API_BASE_URL = '$apiBaseUrl'"
    "`$env:VITE_SIGNALR_HUB_URL = '$apiBaseUrl/hubs/vennusign'"
    'npm run dev -- --host localhost --port 5175'
)

Write-Host ''
Write-Host 'Track 1 owner acceptance services are starting.'
Write-Host "Back Office: $backOfficeBaseUrl"
Write-Host "Display:     $displayBaseUrl/display/74000000-0000-0000-0000-000000000001"
Write-Host ''
Write-Host 'Configured local tokens:'
foreach ($session in $sessions) {
    Write-Host "  $($session.Name): $($session.Token)"
}
Write-Host ''
Write-Host 'After the API applies migrations, load/reset the local fixture with:'
Write-Host 'sqlcmd -S "(localdb)\MSSQLLocalDB" -d VennuSign -E -b -i docs\acceptance\track-1-owner-fixture.sql'
