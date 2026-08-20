[CmdletBinding()]
param(
    [ValidateRange(1, 12)]
    [int]$MaxParallel = 8,
    [ValidateSet('grok-4.5', 'gpt-5.6', 'claude-opus-5', 'gemini-3.5-flash', 'minimax-m3')]
    [string]$BrowserUseModel,
    [string[]]$OnlyLanes,
    [switch]$KeepServices,
    [switch]$SkipBuild,
    [switch]$PrepareOnly
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$runId = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
$runRoot = Join-Path $repoRoot "artifacts\track1-qa\$runId"
$logRoot = Join-Path $runRoot 'logs'
$promptRoot = Join-Path $runRoot 'prompts'
$resultRoot = Join-Path $runRoot 'lanes'
$fixtureRoot = Join-Path $runRoot 'fixtures'
$fixturePath = Join-Path $repoRoot 'docs\acceptance\track-1-owner-fixture.sql'
$startedProcesses = [System.Collections.Generic.List[System.Diagnostics.Process]]::new()
# Hosted runs that are still in flight. Anything left here when the script exits is
# cancelled, because an abandoned hosted run keeps billing after we stop watching it.
$script:activeRunIds = [System.Collections.Generic.List[string]]::new()

# Baseline fixture identities. Every fixture GUID shares the '-0000-0000-0000-' middle,
# so rewriting that one group yields a disjoint dataset per isolation tag.
$baseOrganizationId = '72000000-0000-0000-0000-000000000001'
$baseVenueId = '73000000-0000-0000-0000-000000000001'
$baseScreenId = '74000000-0000-0000-0000-000000000001'
# BaselineToken is what the owner acceptance workbook tells a reviewer to type, so the
# default dataset must keep those exact names. Only the extra isolated datasets get a
# tag suffix, which is enough to stop concurrent lanes sharing an identity.
$baseRoles = @(
    @{ Key = 'owner';     BaselineToken = 'track1-owner-review';   BaseUserId = '71000000-0000-0000-0000-000000000001'; Name = 'Track 1 Owner Review';   Role = 'organization_owner' },
    @{ Key = 'editor';    BaselineToken = 'track1-content-editor'; BaseUserId = '71000000-0000-0000-0000-000000000002'; Name = 'Track 1 Content Editor'; Role = 'content_editor' },
    @{ Key = 'publisher'; BaselineToken = 'track1-publisher';      BaseUserId = '71000000-0000-0000-0000-000000000003'; Name = 'Track 1 Publisher';      Role = 'publisher' }
)

# Only cases a deterministic assertion cannot honestly judge belong here. Everything
# mechanical moved to the Playwright suite in tests/ui, which runs on every commit for
# free; these lanes cost real money per run, so keep the set small and subjective.
#
# Covered by tests/ui instead (do not re-add): 1-0, 1-1, 1-3, 1-4, 2-0, 2-1, 3-0, 3-1,
# 4-0, 6-0.
$laneDefinitions = @(
    @{
        Id = 'copy-quality'; Tag = '0000'; Mutating = $false; Model = 'claude-opus-5'
        Routes = 'schedules, pos'
        Assignments = 'Case 4-1 only, and only the wording. The Playwright suite already proves the temporarily-blocked state is present and attributed. Judge whether the copy reads as a temporary rollout block rather than a breakage, whether it is distinguishable from an entitlement denial, and whether the retry guidance is actionable.'
    },
    @{
        Id = 'localization'; Tag = '0000'; Mutating = $false; Model = 'grok-4.5'
        Routes = 'menu, themes'
        Assignments = 'Translation and fallback case 5-0.'
    },
    @{
        Id = 'responsive'; Tag = '0000'; Mutating = $false; Model = 'grok-4.5'
        Routes = 'home, menu, schedules, screens, billing'
        Assignments = 'Case 6-1 only. Judge the navigation and content layout at a narrow mobile viewport width: reading order, overlap, truncation, and whether anything important is unreachable.'
    },
    @{
        Id = 'shell-quality'; Tag = '0000'; Mutating = $false; Model = 'grok-4.5'
        Routes = 'home, menu, screens, pos'
        Assignments = 'Keyboard-only case 6-2 and state clarity case 6-3 (loading, empty, denied, blocked, offline, error, and recovery language).'
    }
)

if ($OnlyLanes) {
    $laneDefinitions = @($laneDefinitions | Where-Object { $_.Id -in $OnlyLanes })
    if (-not $laneDefinitions) { throw "No lanes matched -OnlyLanes: $($OnlyLanes -join ', ')" }
}

function Get-LaneGuid {
    param([Parameter(Mandatory)][string]$BaseGuid, [Parameter(Mandatory)][string]$Tag)
    return $BaseGuid -replace '-0000-0000-0000-', "-0000-0000-$Tag-"
}

function Get-LaneIdentity {
    param([Parameter(Mandatory)][string]$Tag)
    $roles = @($baseRoles | ForEach-Object {
        [pscustomobject]@{
            Key = $_.Key
            Token = if ($Tag -eq '0000') { $_.BaselineToken } else { "track1-$($_.Key)-$Tag" }
            UserId = Get-LaneGuid -BaseGuid $_.BaseUserId -Tag $Tag
            Name = $_.Name
            Role = $_.Role
        }
    })
    return [pscustomobject]@{
        Tag = $Tag
        OrganizationId = Get-LaneGuid -BaseGuid $baseOrganizationId -Tag $Tag
        VenueId = Get-LaneGuid -BaseGuid $baseVenueId -Tag $Tag
        ScreenId = Get-LaneGuid -BaseGuid $baseScreenId -Tag $Tag
        Roles = $roles
    }
}

function Assert-Command {
    param([Parameter(Mandatory)][string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' is not available on PATH."
    }
}

function Assert-PortAvailable {
    param([Parameter(Mandatory)][int]$Port)
    $listener = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue
    if ($listener) {
        $owners = ($listener | Select-Object -ExpandProperty OwningProcess -Unique) -join ', '
        throw "Port $Port is already in use by PID(s) $owners. Stop that process or use its running environment before starting isolated QA."
    }
}

function Start-OwnedProcess {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$FilePath,
        [Parameter(Mandatory)][string[]]$ArgumentList,
        [Parameter(Mandatory)][string]$WorkingDirectory
    )
    $stdout = Join-Path $logRoot "$Name.out.log"
    $stderr = Join-Path $logRoot "$Name.err.log"
    $process = Start-Process -FilePath $FilePath -ArgumentList $ArgumentList -WorkingDirectory $WorkingDirectory -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru
    $startedProcesses.Add($process)
    return $process
}

function Wait-ForText {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Pattern,
        [int]$TimeoutSeconds = 45
    )
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        if (Test-Path $Path) {
            $match = Select-String -Path $Path -Pattern $Pattern | Select-Object -First 1
            if ($match) { return $match.Matches[0].Value }
        }
        Start-Sleep -Milliseconds 300
    } while ((Get-Date) -lt $deadline)
    throw "Timed out waiting for '$Pattern' in $Path."
}

function Start-QaTunnel {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Origin,
        [switch]$NoTlsVerify
    )
    $tunnelArgs = @('tunnel', '--url', $Origin, '--loglevel', 'info')
    if ($NoTlsVerify) { $tunnelArgs += '--no-tls-verify' }
    $null = Start-OwnedProcess -Name "tunnel-$Name" -FilePath 'cloudflared' -ArgumentList $tunnelArgs -WorkingDirectory $repoRoot
    $url = Wait-ForText -Path (Join-Path $logRoot "tunnel-$Name.err.log") -Pattern 'https://[a-z0-9-]+\.trycloudflare\.com'
    Write-Host "  $Name tunnel: $url"
    return $url
}

function Wait-ForHttp {
    param(
        [Parameter(Mandatory)][string]$Url,
        [int]$TimeoutSeconds = 90
    )
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $statusCode = & curl.exe --insecure --silent --output NUL --write-out '%{http_code}' --max-time 5 $Url
        if ($LASTEXITCODE -eq 0 -and [int]$statusCode -ge 200 -and [int]$statusCode -lt 500) { return }
        Start-Sleep -Milliseconds 500
    } while ((Get-Date) -lt $deadline)
    throw "Timed out waiting for $Url."
}

function New-LaneFixture {
    param([Parameter(Mandatory)][string]$Tag)
    $sql = Get-Content -LiteralPath $fixturePath -Raw
    if ($Tag -ne '0000') {
        # Disjoint identities. Emails and ScreenKey are rewritten too because they carry
        # uniqueness constraints that GUID rewriting alone would violate.
        $sql = $sql -replace '-0000-0000-0000-', "-0000-0000-$Tag-"
        foreach ($role in $baseRoles) {
            # -creplace: NormalizedEmail is the uppercase twin of Email, and a
            # case-insensitive replace would rewrite it in lowercase and break lookups.
            $sql = $sql -creplace "track1-$($role.Key)@local\.vennu\.test", "track1-$($role.Key)-$Tag@local.vennu.test"
            $sql = $sql -creplace "TRACK1-$($role.Key.ToUpperInvariant())@LOCAL\.VENNU\.TEST", "TRACK1-$($role.Key.ToUpperInvariant())-$Tag@LOCAL.VENNU.TEST"
        }
        # UX_Screens_ScreenKey is globally unique and ScreenKey is nvarchar(9), which
        # 'sc-t1demo' already fills exactly, so the tag has to replace characters.
        $screenKey = "sc-t1d$($Tag.Substring($Tag.Length - 3))"
        if ($screenKey.Length -gt 9) { throw "Derived ScreenKey '$screenKey' exceeds nvarchar(9)." }
        $sql = $sql -replace "N'sc-t1demo'", "N'$screenKey'"
    }
    $path = Join-Path $fixtureRoot "fixture-$Tag.sql"
    Set-Content -LiteralPath $path -Value $sql -Encoding utf8
    return $path
}

function Initialize-LaneFixture {
    param([Parameter(Mandatory)][string]$Tag)
    $path = New-LaneFixture -Tag $Tag
    Write-Host "Applying Track 1 fixture for isolation tag $Tag..."
    & sqlcmd -S '(localdb)\MSSQLLocalDB' -d VennuSign -E -b -I -i $path | Tee-Object -FilePath (Join-Path $logRoot "fixture-$Tag.log")
    if ($LASTEXITCODE -ne 0) { throw "Track 1 fixture for tag $Tag failed with exit code $LASTEXITCODE." }
}

function Get-BrowserUseApiKey {
    if ($env:BROWSER_USE_API_KEY) { return $env:BROWSER_USE_API_KEY }
    $status = (& browser-harness auth status | ConvertFrom-Json)
    if ($LASTEXITCODE -ne 0 -or $status.status -ne 'authenticated') {
        throw 'Browser Use is not authenticated. Run browser-harness auth login once.'
    }
    $auth = Get-Content -LiteralPath $status.path -Raw | ConvertFrom-Json
    $key = $auth.browser_use.api_key
    if (-not $key) { throw 'Browser Use authentication does not contain an API key.' }
    return $key
}

function Invoke-BrowserUseApi {
    param(
        [Parameter(Mandatory)][ValidateSet('GET', 'POST')][string]$Method,
        [Parameter(Mandatory)][string]$Path,
        [object]$Body
    )
    $parameters = @{
        Method = $Method
        Uri = "https://api.browser-use.com/api/v4/$Path"
        Headers = @{ 'X-Browser-Use-API-Key' = $script:browserUseApiKey }
        TimeoutSec = 90
    }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json'
        $parameters.Body = $Body | ConvertTo-Json -Depth 12 -Compress
    }
    return Invoke-RestMethod @parameters
}

function New-AgentPrompt {
    param(
        [Parameter(Mandatory)][hashtable]$Lane,
        [Parameter(Mandatory)][object]$Identity,
        [Parameter(Mandatory)][hashtable]$Urls
    )
    $tokenLines = ($Identity.Roles | ForEach-Object { "  $($_.Key) ($($_.Role)): $($_.Token)" }) -join "`n"
    $routeLines = (($Lane.Routes -split ',\s*') | ForEach-Object { "  $_`: $($Urls.BackOffice)/#$_" }) -join "`n"
@"
You are the first-line Vennusign QA Engineer running on Browser Use hosted infrastructure. Do not ask the user questions and do not perform work outside the supplied websites.

Open the exact owner acceptance workbook at $($Urls.Workbook) and test only this lane: $($Lane.Assignments)
Back Office: $($Urls.BackOffice)
API: $($Urls.Api)
Display: $($Urls.Display)/display/$($Identity.ScreenId)

The workbook is the source of truth for each assigned case's detailed steps and expected evidence. Use its numeric case IDs shown in the assignment. Replace localhost URLs in workbook commands with the supplied public API URL when driving the cloud environment.

ACCESS TOKENS FOR THIS LANE. These override any token names printed in the workbook, because this lane runs against its own isolated dataset:
$tokenLines

WORK EFFICIENTLY. Every extra page observation costs real time and money, so:
- Navigate by URL directly. The Back Office uses hash routes; go straight to the route you need instead of clicking through navigation:
$routeLines
- Read the workbook once, extract the steps for all of your assigned cases, then execute. Do not re-open the workbook between cases.
- Do not take a screenshot to confirm something a targeted DOM or text check already tells you. Screenshot only when the case is genuinely about layout, imagery, or visual state.
- Do not explore the application beyond your assigned cases. Do not repeat a passing check for reassurance.
- If an element is genuinely absent after one deliberate look, that is a finding to record, not a reason to keep searching.

For every assigned case, return one result. Use Pass only after observing the expected result. Use Fail for a definite functional failure, Needs Adjustment for a usable but materially deficient experience, and Manual Review only when the judgment cannot honestly be automated. Notes must state what was observed and evidence must identify screenshots or exact UI/API evidence. Do not claim a click succeeded without verifying the resulting state.

Return JSON only, with no Markdown fence or commentary. It must match this exact shape:
{"schema":"vennusign.track1.qa-lane","version":1,"laneId":"$($Lane.Id)","startedAt":"ISO-8601 UTC","completedAt":"ISO-8601 UTC","summary":"concise summary","cases":[{"caseId":"0-0","title":"exact workbook case title","status":"Pass|Fail|Needs Adjustment|Manual Review","notes":"specific observation","evidence":["specific page, UI text, API value, screenshot, or recording observation"]}]}
"@
}

function Start-QaHostedRun {
    param(
        [Parameter(Mandatory)][hashtable]$Lane,
        [Parameter(Mandatory)][hashtable]$Urls
    )
    $identity = Get-LaneIdentity -Tag $Lane.Tag
    $prompt = New-AgentPrompt -Lane $Lane -Identity $identity -Urls $Urls
    Set-Content -LiteralPath (Join-Path $promptRoot "$($Lane.Id).txt") -Value $prompt -Encoding utf8
    $model = if ($BrowserUseModel) { $BrowserUseModel } else { $Lane.Model }
    Write-Host "Dispatching lane '$($Lane.Id)' (tag $($Lane.Tag), model $model)"
    $run = Invoke-BrowserUseApi -Method POST -Path 'runs' -Body @{
        task = $prompt
        model = $model
        browserSettings = @{ proxyCountryCode = $null; record = $true }
    }
    $run | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $resultRoot "$($Lane.Id).run.json") -Encoding utf8
    $script:activeRunIds.Add([string]$run.id)
    return [pscustomobject]@{ LaneId = $Lane.Id; RunId = $run.id; Model = $model; StartedUtc = (Get-Date).ToUniversalTime() }
}

function Complete-QaHostedRun {
    param([Parameter(Mandatory)][object]$Run)
    $completed = Invoke-BrowserUseApi -Method GET -Path "runs/$($Run.RunId)"
    $completed | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $resultRoot "$($Run.LaneId).completed.json") -Encoding utf8
    $resultText = ([string]$completed.result).Trim()
    if ($resultText.StartsWith('```')) {
        $resultText = $resultText -replace '^```(?:json)?\s*', '' -replace '\s*```$', ''
    }
    # Some models wrap the contract in prose. Salvage the object when one is present
    # rather than discarding a lane's whole run over a preamble.
    $open = $resultText.IndexOf('{')
    $close = $resultText.LastIndexOf('}')
    if ($open -ge 0 -and $close -gt $open) { $resultText = $resultText.Substring($open, $close - $open + 1) }
    try { $laneResult = $resultText | ConvertFrom-Json } catch { throw "returned no usable JSON. Raw result: $(([string]$completed.result).Trim())" }
    if ($laneResult.schema -ne 'vennusign.track1.qa-lane' -or $laneResult.version -ne 1 -or $laneResult.laneId -ne $Run.LaneId) {
        throw 'returned an invalid lane result contract.'
    }
    if (-not $laneResult.cases -or @($laneResult.cases).Count -eq 0) {
        throw 'returned zero cases, so the lane did no verifiable testing.'
    }
    $laneResult | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $resultRoot "$($Run.LaneId).json") -Encoding utf8
    $elapsed = [Math]::Round(((Get-Date).ToUniversalTime() - $Run.StartedUtc).TotalMinutes, 1)
    Write-Host ('Completed lane ''{0}'' in {1} min (in {2} tok, out {3} tok, ${4:N2})' -f $Run.LaneId, $elapsed, $completed.totalInputTokens, $completed.totalOutputTokens, $completed.totalCostUsd)
    return [pscustomobject]@{
        LaneId = $Run.LaneId; Minutes = $elapsed; Model = $Run.Model
        InputTokens = $completed.totalInputTokens; OutputTokens = $completed.totalOutputTokens; CostUsd = $completed.totalCostUsd
    }
}

function Invoke-QaLanes {
    param(
        [Parameter(Mandatory)][object[]]$Lanes,
        [Parameter(Mandatory)][hashtable]$Urls,
        [int]$TimeoutMinutes = 30
    )
    $queue = [System.Collections.Generic.Queue[object]]::new()
    foreach ($lane in $Lanes) { $queue.Enqueue($lane) }
    $active = [System.Collections.Generic.List[object]]::new()
    $telemetry = [System.Collections.Generic.List[object]]::new()
    $failures = [System.Collections.Generic.List[object]]::new()
    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)

    # One lane's bad output must never abandon the other lanes mid-flight: that wastes
    # their work and leaves hosted runs billing. Every lane outcome is isolated here.
    while (($queue.Count -gt 0 -or $active.Count -gt 0) -and (Get-Date) -lt $deadline) {
        while ($queue.Count -gt 0 -and $active.Count -lt $MaxParallel) {
            $lane = $queue.Dequeue()
            try { $active.Add((Start-QaHostedRun -Lane $lane -Urls $Urls)) }
            catch {
                $failures.Add([pscustomobject]@{ LaneId = $lane.Id; Error = "dispatch failed: $($_.Exception.Message)" })
                Write-Warning "Lane '$($lane.Id)' dispatch failed: $($_.Exception.Message)"
            }
        }
        Start-Sleep -Seconds 3
        foreach ($run in @($active)) {
            try { $state = Invoke-BrowserUseApi -Method GET -Path "runs/$($run.RunId)/status" } catch { continue }
            if ($state.status -notin @('completed', 'failed', 'cancelled')) { continue }
            try {
                if ($state.status -ne 'completed') { throw "run ended with status '$($state.status)'." }
                $telemetry.Add((Complete-QaHostedRun -Run $run))
            } catch {
                $failures.Add([pscustomobject]@{ LaneId = $run.LaneId; Error = $_.Exception.Message })
                Write-Warning "Lane '$($run.LaneId)' $($_.Exception.Message)"
            }
            $null = $active.Remove($run)
            $null = $script:activeRunIds.Remove($run.RunId)
        }
    }
    foreach ($run in @($active)) {
        $failures.Add([pscustomobject]@{ LaneId = $run.LaneId; Error = "timed out after $TimeoutMinutes min" })
    }
    foreach ($lane in @($queue)) {
        $failures.Add([pscustomobject]@{ LaneId = $lane.Id; Error = 'never dispatched before timeout' })
    }
    return [pscustomobject]@{ Telemetry = $telemetry; Failures = $failures }
}

function Merge-QaResults {
    param([object[]]$LaneIds, [object[]]$Telemetry, [object[]]$Failures)
    if (-not $LaneIds -or @($LaneIds).Count -eq 0) { throw 'No lane produced a usable result; nothing to merge.' }
    $laneResults = @($LaneIds | ForEach-Object {
        $path = Join-Path $resultRoot "$_.json"
        if (-not (Test-Path $path)) { throw "Missing QA lane result: $path" }
        Get-Content $path -Raw | ConvertFrom-Json
    })
    $allCases = @($laneResults | ForEach-Object { $_.cases })
    $duplicates = @($allCases | Group-Object caseId | Where-Object Count -gt 1)
    if ($duplicates) { throw "Duplicate QA case results: $(($duplicates.Name) -join ', ')." }

    # Only the subjective cases are expected here now. The rest are asserted by the
    # Playwright suite in tests/ui and are not a coverage gap when absent.
    $expected = @('4-1', '5-0', '6-1', '6-2', '6-3')
    $missing = @($expected | Where-Object { $_ -notin $allCases.caseId })
    if ($missing -and -not $OnlyLanes -and -not $Failures) {
        throw "Missing QA case results: $($missing -join ', ')."
    }
    if ($missing) { Write-Warning "Coverage gap - no result for case(s): $($missing -join ', ')" }

    $attention = @($allCases | Where-Object status -in @('Fail', 'Needs Adjustment'))
    $manual = @($allCases | Where-Object status -eq 'Manual Review')
    $results = @{}
    $notes = @{}
    foreach ($case in $allCases) {
        if ($case.status -ne 'Manual Review') { $results[$case.caseId] = $case.status }
        $evidence = if ($case.evidence.Count) { " Evidence: $($case.evidence -join '; ')" } else { '' }
        $notes[$case.caseId] = "QA $($case.status): $($case.notes)$evidence"
    }
    $notes['7-0'] = "QA gate summary: $($attention.Count) attention case(s); $($manual.Count) manual-review case(s). Owner product-intent judgment remains required."

    $acceptanceRecord = [ordered]@{
        schema = 'vennusign.track1.owner-acceptance'
        version = 2
        savedAt = (Get-Date).ToUniversalTime().ToString('o')
        fields = [ordered]@{
            reviewer = 'Codex QA agents'
            reviewDate = (Get-Date).ToString('yyyy-MM-dd')
            environment = "Automated local QA $runId"
            closure = ''
            finalNotes = "QA preflight only. Owner acceptance remains the closure authority. $($attention.Count) attention case(s); $($manual.Count) manual-review case(s)."
        }
        results = $results
        notes = $notes
        qa = [ordered]@{ runId = $runId; lanes = $laneResults; attentionCount = $attention.Count; manualReviewCount = $manual.Count; telemetry = $Telemetry; laneFailures = $Failures; missingCases = $missing }
    }
    $recordPath = Join-Path $runRoot 'track-1-owner-acceptance.qa.json'
    $acceptanceRecord | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $recordPath -Encoding utf8
    Write-Host "QA acceptance record: $recordPath"
    return [pscustomobject]@{ RecordPath = $recordPath; AttentionCount = $attention.Count; ManualReviewCount = $manual.Count }
}

New-Item -ItemType Directory -Force -Path $logRoot, $promptRoot, $resultRoot, $fixtureRoot | Out-Null

try {
    foreach ($command in @('browser-harness', 'cloudflared', 'curl.exe', 'dotnet', 'node', 'npm', 'python', 'sqlcmd')) { Assert-Command $command }
    $script:browserUseApiKey = Get-BrowserUseApiKey
    foreach ($port in @(7138, 5174, 5175, 5176)) { Assert-PortAvailable $port }

    if (-not $SkipBuild) {
        # Validation only: the QA run itself serves the Vite dev servers and a `dotnet run`
        # Debug build, so none of this output is on the tested path.
        Write-Host 'Building affected applications...'
        & dotnet build (Join-Path $repoRoot 'src\Vennu.Api\Vennu.Api.csproj') -c Release --nologo
        if ($LASTEXITCODE -ne 0) { throw 'API Release build failed.' }
        Push-Location (Join-Path $repoRoot 'src\back-office')
        try { & npm run build; if ($LASTEXITCODE -ne 0) { throw 'Back Office build failed.' } } finally { Pop-Location }
        Push-Location (Join-Path $repoRoot 'src\display')
        try { & npm run build; if ($LASTEXITCODE -ne 0) { throw 'Display build failed.' } } finally { Pop-Location }
    }

    Write-Host 'Creating isolated public QA endpoints...'
    $apiPublic = Start-QaTunnel -Name 'api' -Origin 'https://localhost:7138' -NoTlsVerify
    $backOfficePublic = Start-QaTunnel -Name 'back-office' -Origin 'https://localhost:5174' -NoTlsVerify
    $displayPublic = Start-QaTunnel -Name 'display' -Origin 'http://localhost:5175'
    $workbookPublic = Start-QaTunnel -Name 'workbook' -Origin 'http://localhost:5176'

    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = 'https://localhost:7138'
    $env:Cors__AllowedOrigins__0 = $backOfficePublic
    $env:Cors__AllowedOrigins__1 = $displayPublic

    # One session per role per isolation tag, so concurrent lanes never share a signed-in identity.
    $tags = @($laneDefinitions | ForEach-Object { $_.Tag } | Sort-Object -Unique)
    $sessionIndex = 0
    foreach ($tag in $tags) {
        $identity = Get-LaneIdentity -Tag $tag
        foreach ($role in $identity.Roles) {
            Set-Item "env:BackOffice__Sessions__${sessionIndex}__AccessToken" $role.Token
            Set-Item "env:BackOffice__Sessions__${sessionIndex}__VenueId" $identity.VenueId
            Set-Item "env:BackOffice__Sessions__${sessionIndex}__OrganizationId" $identity.OrganizationId
            Set-Item "env:BackOffice__Sessions__${sessionIndex}__UserId" $role.UserId
            Set-Item "env:BackOffice__Sessions__${sessionIndex}__DisplayName" "$($role.Name) [$tag]"
            Set-Item "env:BackOffice__Sessions__${sessionIndex}__SystemRole" $role.Role
            $sessionIndex++
        }
    }
    Write-Host "Configured $sessionIndex Back Office sessions across isolation tag(s): $($tags -join ', ')"

    $null = Start-OwnedProcess -Name 'api' -FilePath 'dotnet' -ArgumentList @('run', '--no-launch-profile', '--project', '.\src\Vennu.Api\Vennu.Api.csproj') -WorkingDirectory $repoRoot

    $env:VITE_API_URL = $apiPublic
    $env:VITE_DISPLAY_URL = $displayPublic
    $null = Start-OwnedProcess -Name 'back-office' -FilePath 'npm.cmd' -ArgumentList @('run', 'dev', '--', '--host', 'localhost', '--port', '5174') -WorkingDirectory (Join-Path $repoRoot 'src\back-office')

    $env:VITE_API_URL = $apiPublic
    $env:VITE_SIGNALR_URL = "$apiPublic/hubs/vennusign"
    $null = Start-OwnedProcess -Name 'display' -FilePath 'npm.cmd' -ArgumentList @('run', 'dev', '--', '--host', 'localhost', '--port', '5175') -WorkingDirectory (Join-Path $repoRoot 'src\display')
    $null = Start-OwnedProcess -Name 'workbook' -FilePath 'python' -ArgumentList @('-m', 'http.server', '5176', '--bind', 'localhost', '--directory', (Join-Path $repoRoot 'docs\acceptance')) -WorkingDirectory $repoRoot

    Wait-ForHttp 'https://localhost:7138/health/version'
    Wait-ForHttp 'https://localhost:5174'
    Wait-ForHttp 'http://localhost:5175'
    Wait-ForHttp 'http://localhost:5176/track-1-owner-acceptance.html'

    # Every tag is seeded once, up front. Because lanes own disjoint data, no fixture is
    # re-applied mid-run and no lane has to wait for another lane to finish.
    foreach ($tag in $tags) { Initialize-LaneFixture -Tag $tag }

    $urls = @{ Api = $apiPublic; BackOffice = $backOfficePublic; Display = $displayPublic; Workbook = "$workbookPublic/track-1-owner-acceptance.html" }
    $urls | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $runRoot 'endpoints.json') -Encoding utf8
    Write-Host "Owner workbook: $($urls.Workbook)"

    if ($PrepareOnly) {
        Write-Host 'Preparation complete. Agent execution was skipped.'
        if (-not $KeepServices) { Write-Warning 'Use -KeepServices with -PrepareOnly to keep these endpoints available.' }
        return
    }

    $runStart = Get-Date
    $outcome = Invoke-QaLanes -Lanes $laneDefinitions -Urls $urls
    $telemetry = @($outcome.Telemetry)
    $failures = @($outcome.Failures)
    $wallClock = [Math]::Round(((Get-Date) - $runStart).TotalMinutes, 1)

    if ($failures.Count) {
        Write-Host ''
        Write-Host "Lane failures ($($failures.Count)):"
        foreach ($failure in $failures) { Write-Host "  $($failure.LaneId): $($failure.Error)" }
        Write-Host "Re-run just these with: -OnlyLanes $(($failures.LaneId) -join ',')"
    }

    $merged = Merge-QaResults -LaneIds @($telemetry | ForEach-Object { $_.LaneId }) -Telemetry $telemetry -Failures $failures
    $totalCost = ($telemetry | Measure-Object -Property CostUsd -Sum).Sum
    $laneMinutes = ($telemetry | Measure-Object -Property Minutes -Sum).Sum
    Write-Host ''
    Write-Host ('Wall clock {0} min across {1} lane(s); {2} lane-minutes of agent work; total cost ${3:N2}' -f $wallClock, $telemetry.Count, $laneMinutes, $totalCost)

    if ($merged.AttentionCount -gt 0) {
        Write-Error "QA gate failed with $($merged.AttentionCount) case(s) needing attention. Import $($merged.RecordPath) into the owner workbook for full evidence."
    } else {
        Write-Host "QA gate passed automated checks. $($merged.ManualReviewCount) case(s) still require honest manual/owner judgment."
        Write-Host "Import $($merged.RecordPath) into the owner workbook."
    }
}
finally {
    # Always cancel in-flight hosted runs, even with -KeepServices: an abandoned run
    # keeps billing long after this script stops watching it.
    if ($script:browserUseApiKey -and $script:activeRunIds.Count -gt 0) {
        Write-Host "Cancelling $($script:activeRunIds.Count) in-flight hosted run(s) to stop billing..."
        foreach ($activeRunId in @($script:activeRunIds)) {
            try { $null = Invoke-BrowserUseApi -Method POST -Path "runs/$activeRunId/cancel" }
            catch { Write-Warning "Could not cancel run ${activeRunId}: $($_.Exception.Message)" }
        }
    }
    if (-not $KeepServices) {
        Write-Host 'Stopping only QA-owned services and tunnels...'
        foreach ($process in ($startedProcesses | Sort-Object Id -Descending)) {
            if (-not $process.HasExited) {
                & taskkill.exe /PID $process.Id /T /F 2>$null | Out-Null
            }
        }
    } else {
        Write-Host "QA services remain running. Owned PIDs: $(($startedProcesses.Id) -join ', ')"
    }
}
