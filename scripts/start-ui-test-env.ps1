<#
.SYNOPSIS
    Starts a purely local environment for the Playwright UI suite in tests/ui.

.DESCRIPTION
    Deliberately does NOT create cloudflared tunnels. The hosted-agent QA harness
    (run-track1-qa.ps1) has to expose public URLs and therefore configures CORS for
    those tunnel origins, which blocks a browser running on localhost. Playwright
    drives a real browser locally, so it needs localhost in the allowed origins and
    the front end pointed at the localhost API.

    Leaves services running. Stop them with -Stop.
#>
[CmdletBinding()]
param(
    [string]$IsolationTag = '0000',
    [switch]$SkipFixture,
    [switch]$Stop,
    [switch]$PruneSeed,
    # The display dev server's port. Overridable because 5175 is a popular default
    # and something else on the machine may already own it; nothing here depends on
    # the number, only on the API and the front ends agreeing on it.
    [int]$DisplayPort = 5175
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$logRoot = Join-Path $repoRoot 'artifacts\ui-test-env'
$apiOrigin = 'https://localhost:7138'
$testApiOrigin = 'https://localhost:7140'
$backOfficeOrigin = 'https://localhost:5174'
$displayOrigin = "http://localhost:$DisplayPort"
$ports = @(7138, 7140, 5174, $DisplayPort)

function Stop-UiTestEnv {
    foreach ($port in $ports) {
        $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
        foreach ($owner in ($listener.OwningProcess | Select-Object -Unique)) {
            & taskkill.exe /PID $owner /T /F 2>$null | Out-Null
            Write-Host "Stopped PID $owner on port $port"
        }
    }
}

if ($Stop) { Stop-UiTestEnv; return }

if ($PruneSeed) {
    # Rows created by POST /api/test/seed accumulate with every run. They are
    # identifiable: seeded screens use a 't' + 8 hex ScreenKey and seeded menus end
    # with the same 8 hex suffix. Deliberately kept out of the API so the deployed
    # system carries no delete-by-pattern endpoint.
    $prune = @"
SET NOCOUNT ON;
DECLARE @Menus TABLE (Id uniqueidentifier);
INSERT INTO @Menus (Id)
SELECT Id FROM dbo.Menus WHERE Name LIKE '% menu [0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f]';

-- Seeded content now lives in the item library, so the placements go first,
-- then the library items they were the only reason to keep.
DECLARE @SeedItems TABLE (Id uniqueidentifier);
INSERT INTO @SeedItems (Id)
SELECT DISTINCT p.ItemId FROM dbo.Placements p INNER JOIN @Menus m ON m.Id = p.MenuId;

-- A paste-review session may have matched one of these seeded library items.
-- It is temporary test state and owns the candidate FK, so remove the complete
-- session before pruning the item. This mirrors production expiry cleanup.
DECLARE @ImportSessions TABLE (Id uniqueidentifier PRIMARY KEY);
INSERT INTO @ImportSessions (Id)
SELECT DISTINCT c.SessionId
FROM dbo.MenuImportCandidates c INNER JOIN @SeedItems i ON i.Id = c.ItemId;
INSERT INTO @ImportSessions (Id)
SELECT s.Id FROM dbo.MenuImportSessions s INNER JOIN @Menus m ON m.Id IN (s.TargetMenuId,s.CompletedMenuId)
WHERE NOT EXISTS (SELECT 1 FROM @ImportSessions existing WHERE existing.Id=s.Id);
DELETE cl FROM dbo.MenuImportCreatedLines cl INNER JOIN @ImportSessions s ON s.Id=cl.SessionId;
DELETE a FROM dbo.MenuImportAnswers a INNER JOIN @ImportSessions s ON s.Id = a.SessionId;
DELETE c FROM dbo.MenuImportCandidates c INNER JOIN @ImportSessions s ON s.Id = c.SessionId;
DELETE ql FROM dbo.MenuImportQuestionLines ql INNER JOIN @ImportSessions s ON s.Id = ql.SessionId;
DELETE q FROM dbo.MenuImportReviewQuestions q INNER JOIN @ImportSessions s ON s.Id = q.SessionId;
DELETE l FROM dbo.MenuImportSourceLines l INNER JOIN @ImportSessions s ON s.Id = l.SessionId;
UPDATE x SET CompletedSnapshotId=NULL FROM dbo.MenuImportSessions x INNER JOIN @ImportSessions s ON s.Id=x.Id;
DELETE snap FROM dbo.MenuImportReplacementSnapshots snap
WHERE snap.SessionId IN (SELECT Id FROM @ImportSessions) OR snap.MenuId IN (SELECT Id FROM @Menus);
DELETE x FROM dbo.MenuImportSessions x INNER JOIN @ImportSessions s ON s.Id = x.Id;

DELETE p FROM dbo.Placements p INNER JOIN @Menus m ON m.Id = p.MenuId;

DELETE a FROM dbo.ItemAvailability a
INNER JOIN @SeedItems i ON i.Id = a.ItemId
WHERE NOT EXISTS (SELECT 1 FROM dbo.Placements p WHERE p.ItemId = a.ItemId);

DELETE x FROM dbo.Items x
INNER JOIN @SeedItems i ON i.Id = x.Id
WHERE NOT EXISTS (SELECT 1 FROM dbo.Placements p WHERE p.ItemId = x.Id);

-- The publish chain references the menu, so it goes before the menu itself.
DELETE h FROM dbo.MenuHistoryEntries h INNER JOIN @Menus m ON m.Id = h.MenuId;
DELETE t FROM dbo.MenuPublishTargets t
INNER JOIN dbo.MenuPublishEvents e ON e.Id = t.PublishEventId
INNER JOIN @Menus m ON m.Id = e.MenuId;
DELETE e FROM dbo.MenuPublishEvents e INNER JOIN @Menus m ON m.Id = e.MenuId;
DELETE a FROM dbo.MenuScreenAssignments a INNER JOIN @Menus m ON m.Id = a.MenuId;

DELETE i FROM dbo.MenuItems i
INNER JOIN dbo.MenuSections s ON s.Id = i.MenuSectionId
INNER JOIN @Menus m ON m.Id = s.MenuId;

DELETE s FROM dbo.MenuSections s INNER JOIN @Menus m ON m.Id = s.MenuId;
DELETE m FROM dbo.Menus m INNER JOIN @Menus t ON t.Id = m.Id;

DECLARE @Screens TABLE (Id uniqueidentifier);
INSERT INTO @Screens (Id)
SELECT Id FROM dbo.Screens
WHERE ScreenKey LIKE 't[0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f]'
   OR Name LIKE '% screen [0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f]';

-- Every table with an FK to dbo.Screens must be cleared first.
UPDATE dbo.CustomerOnboardingStates SET FirstScreenId = NULL
WHERE FirstScreenId IN (SELECT Id FROM @Screens);
DELETE d FROM dbo.ScreenContentDeliveries d INNER JOIN @Screens s ON s.Id = d.ScreenId;
DELETE p FROM dbo.ScreenPairingCodes p INNER JOIN @Screens s ON s.Id = p.ScreenId;
DELETE b FROM dbo.EmergencyBroadcasts b INNER JOIN @Screens s ON s.Id = b.ScreenId;
DELETE l FROM dbo.PlaylistSlides l INNER JOIN @Screens s ON s.Id = l.ScreenId;
DELETE a FROM dbo.ScreenReplacementAudits a INNER JOIN @Screens s ON s.Id IN (a.TargetScreenId, a.SourceScreenId);
DELETE x FROM dbo.Screens x INNER JOIN @Screens s ON s.Id = x.Id;

SELECT (SELECT COUNT(*) FROM dbo.Menus) AS MenusLeft, (SELECT COUNT(*) FROM dbo.Screens) AS ScreensLeft;
"@
    Write-Host 'Pruning seeded UI-test rows...'
    & sqlcmd -S '(localdb)\MSSQLLocalDB' -d VennuSign -E -b -I -Q $prune
    if ($LASTEXITCODE -ne 0) { throw "Seed prune failed with exit code $LASTEXITCODE." }
    return
}

foreach ($command in @('dotnet', 'npm', 'sqlcmd')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) { throw "Required command '$command' is not on PATH." }
}
foreach ($port in $ports) {
    $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
    if ($listener) { throw "Port $port is already in use by PID(s) $(($listener.OwningProcess | Select-Object -Unique) -join ', '). Run with -Stop first." }
}

New-Item -ItemType Directory -Force -Path $logRoot | Out-Null
$testApiKeyBytes = New-Object byte[] 32
$testApiKeyGenerator = [Security.Cryptography.RandomNumberGenerator]::Create()
try { $testApiKeyGenerator.GetBytes($testApiKeyBytes) } finally { $testApiKeyGenerator.Dispose() }
$testApiKey = ([BitConverter]::ToString($testApiKeyBytes) -replace '-', '').ToLowerInvariant()
Set-Content -LiteralPath (Join-Path $logRoot 'test-api.key') -Value $testApiKey -NoNewline

function Start-EnvProcess {
    param([string]$Name, [string]$FilePath, [string[]]$ArgumentList, [string]$WorkingDirectory)
    Start-Process -FilePath $FilePath -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $logRoot "$Name.out.log") `
        -RedirectStandardError (Join-Path $logRoot "$Name.err.log") -PassThru
}

function Wait-ForHttp {
    param([string]$Url, [int]$TimeoutSeconds = 120)
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $status = & curl.exe --insecure --silent --output NUL --write-out '%{http_code}' --max-time 5 $Url
        if ($LASTEXITCODE -eq 0 -and [int]$status -ge 200 -and [int]$status -lt 500) { return }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    throw "Timed out waiting for $Url."
}

# Seeded identities match run-track1-qa.ps1 so both harnesses share one fixture.
$guid = { param($base) $base -replace '-0000-0000-0000-', "-0000-0000-$IsolationTag-" }
# BaselineToken matches the owner acceptance workbook. Only non-default datasets get a
# tag suffix, so a reviewer can sign in with exactly what the workbook prints.
# The scale role is an owner of a SECOND venue, so the Menus shelf at scale can be
# deterministic (Q176). The default venue accumulates menus from every spec that
# seeds, so nothing there can assert "exactly this many" while the suite runs in
# parallel; only the scale seed writes to this one, and it clears it first.
$roles = @(
    @{ Key = 'owner';     BaselineToken = 'track1-owner-review';   Base = '71000000-0000-0000-0000-000000000001'; Name = 'Track 1 Owner Review';   Role = 'organization_owner'; Venue = '73000000-0000-0000-0000-000000000001' },
    @{ Key = 'editor';    BaselineToken = 'track1-content-editor'; Base = '71000000-0000-0000-0000-000000000002'; Name = 'Track 1 Content Editor'; Role = 'content_editor';     Venue = '73000000-0000-0000-0000-000000000001' },
    @{ Key = 'publisher'; BaselineToken = 'track1-publisher';      Base = '71000000-0000-0000-0000-000000000003'; Name = 'Track 1 Publisher';      Role = 'publisher';          Venue = '73000000-0000-0000-0000-000000000001' },
    @{ Key = 'scale';     BaselineToken = 'track1-scale-check';    Base = '71000000-0000-0000-0000-000000000004'; Name = 'Track 1 Scale Check';    Role = 'organization_owner'; Venue = '73000000-0000-0000-0000-000000000002' }
    @{ Key = 'capacity';  BaselineToken = 'track1-capacity-check'; Base = '71000000-0000-0000-0000-000000000005'; Name = 'Track 1 Capacity Check'; Role = 'organization_owner'; Venue = '73000000-0000-0000-0000-000000000003' }
)

$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = $apiOrigin
# The whole point of this script: localhost is the browser's origin.
$env:Cors__AllowedOrigins__0 = $backOfficeOrigin
$env:Cors__AllowedOrigins__1 = $displayOrigin
$env:TestAutomation__ApiKey = $testApiKey
$env:TestAutomation__Scopes__0 = 'availability.backdate'
$env:TestAutomation__Scopes__1 = 'venue.reset'
$env:TestAutomation__Scopes__2 = 'history.write_at'
$env:TestAutomation__AvailabilityVenueIds__0 = (& $guid '73000000-0000-0000-0000-000000000001')
$env:TestAutomation__AvailabilityVenueIds__1 = (& $guid '73000000-0000-0000-0000-000000000002')
$env:TestAutomation__Scopes__3 = 'venue.headroom'
# Reset WIPES a venue, so it stays allowed only on the scale venue, which one test owns at a time.
# Headroom destroys nothing and is needed on the SHARED venue - the one all 98 seeds fill, and the
# only one that ever ran out. Giving headroom the reset scope would have handed every seed the
# power to wipe a venue other tests were using.
$env:TestAutomation__ResetVenueIds__0 = (& $guid '73000000-0000-0000-0000-000000000002')
$env:TestAutomation__HeadroomVenueIds__0 = (& $guid '73000000-0000-0000-0000-000000000001')
$env:TestAutomation__HeadroomVenueIds__1 = (& $guid '73000000-0000-0000-0000-000000000002')
$env:TestAutomation__HistoryVenueIds__0 = (& $guid '73000000-0000-0000-0000-000000000001')
$env:TestAutomation__HistoryVenueIds__1 = (& $guid '73000000-0000-0000-0000-000000000002')

for ($index = 0; $index -lt $roles.Count; $index++) {
    $role = $roles[$index]
    $token = if ($IsolationTag -eq '0000') { $role.BaselineToken } else { "track1-$($role.Key)-$IsolationTag" }
    Set-Item "env:BackOffice__Sessions__${index}__AccessToken" $token
    Set-Item "env:BackOffice__Sessions__${index}__VenueId" (& $guid $role.Venue)
    Set-Item "env:BackOffice__Sessions__${index}__OrganizationId" (& $guid '72000000-0000-0000-0000-000000000001')
    Set-Item "env:BackOffice__Sessions__${index}__UserId" (& $guid $role.Base)
    Set-Item "env:BackOffice__Sessions__${index}__DisplayName" "$($role.Name) [$IsolationTag]"
    Set-Item "env:BackOffice__Sessions__${index}__SystemRole" $role.Role
}

Write-Host 'Starting API...'
$null = Start-EnvProcess -Name 'api' -FilePath 'dotnet' -ArgumentList @('run', '--no-launch-profile', '--project', '.\src\Vennu.Api\Vennu.Api.csproj') -WorkingDirectory $repoRoot

$env:ASPNETCORE_URLS = $testApiOrigin
$env:TestApi__ApiKey = $testApiKey
$env:TestApi__ProductApiBaseUrl = $apiOrigin
$env:TestApi__ProductAutomationKey = $testApiKey
# The one place this is switched on, and it is switched on HERE rather than trusted on the machine.
#
# The Test API calls the product API at https://localhost:7138 with an HttpClient, which validates
# the certificate chain. The ASP.NET Core development certificate is self-signed, so anywhere that
# has never trusted it every seed returned 500 (UntrustedRoot) and the entire UI suite died in
# fixture setup - on every branch, for months, with nobody able to tell a real regression from the
# noise.
#
# `dotnet dev-certs https --trust` is the obvious answer and it hangs a headless runner: adding to
# the Windows Root store raises a confirmation dialog. It also makes a green suite a property of
# how a machine was once set up rather than of the code.
#
# The setting is refused for anything that is not loopback, so this cannot become "accept any
# certificate". See src/Vennu.TestApi/LoopbackCertificateTrust.cs.
$env:TestApi__AllowUntrustedLoopbackCertificate = 'true'
Write-Host 'Starting Test API...'
$null = Start-EnvProcess -Name 'test-api' -FilePath 'dotnet' -ArgumentList @('run', '--no-launch-profile', '--project', '.\src\Vennu.TestApi\Vennu.TestApi.csproj') -WorkingDirectory $repoRoot

$env:VITE_API_URL = $apiOrigin
$env:VITE_DISPLAY_URL = $displayOrigin
Write-Host 'Starting Back Office...'
$null = Start-EnvProcess -Name 'back-office' -FilePath 'npm.cmd' -ArgumentList @('run', 'dev', '--', '--host', 'localhost', '--port', '5174') -WorkingDirectory (Join-Path $repoRoot 'src\back-office')

$env:VITE_API_URL = $apiOrigin
$env:VITE_SIGNALR_URL = "$apiOrigin/hubs/vennusign"
Write-Host 'Starting Display...'
$null = Start-EnvProcess -Name 'display' -FilePath 'npm.cmd' -ArgumentList @('run', 'dev', '--', '--host', 'localhost', '--port', "$DisplayPort") -WorkingDirectory (Join-Path $repoRoot 'src\display')

Wait-ForHttp "$apiOrigin/health/version"
Wait-ForHttp "$testApiOrigin/health/version"
Wait-ForHttp $backOfficeOrigin
Wait-ForHttp $displayOrigin

if (-not $SkipFixture) {
    $fixture = Join-Path $repoRoot 'docs\acceptance\track-1-owner-fixture.sql'
    $sql = Get-Content -LiteralPath $fixture -Raw
    if ($IsolationTag -ne '0000') {
        $sql = $sql -replace '-0000-0000-0000-', "-0000-0000-$IsolationTag-"
        foreach ($role in $roles) {
            $sql = $sql -creplace "track1-$($role.Key)@local\.vennu\.test", "track1-$($role.Key)-$IsolationTag@local.vennu.test"
            $sql = $sql -creplace "TRACK1-$($role.Key.ToUpperInvariant())@LOCAL\.VENNU\.TEST", "TRACK1-$($role.Key.ToUpperInvariant())-$IsolationTag@LOCAL.VENNU.TEST"
        }
        $sql = $sql -replace "N'sc-t1demo'", "N'sc-t1d$($IsolationTag.Substring($IsolationTag.Length - 3))'"
        $sql = $sql -replace "N'sc-cap001'", "N'sc-ca$($IsolationTag.Substring($IsolationTag.Length - 4))'"
    }
    $generated = Join-Path $logRoot "fixture-$IsolationTag.sql"
    Set-Content -LiteralPath $generated -Value $sql -Encoding utf8
    Write-Host "Applying fixture for isolation tag $IsolationTag..."
    & sqlcmd -S '(localdb)\MSSQLLocalDB' -d VennuSign -E -b -I -i $generated | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Fixture failed with exit code $LASTEXITCODE." }
}

Write-Host ''
Write-Host "API:         $apiOrigin"
Write-Host "Test API:    $testApiOrigin"
Write-Host "Back Office: $backOfficeOrigin"
Write-Host "Display:     $displayOrigin"
Write-Host "Logs:        $logRoot"
Write-Host ''
Write-Host 'Run the UI suite:  cd tests\ui; npx playwright test'
Write-Host 'Stop services:     scripts\start-ui-test-env.ps1 -Stop'
