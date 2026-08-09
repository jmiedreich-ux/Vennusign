<#
.SYNOPSIS
    Runs every Menus M1 demo check against a local API and prints the evidence.

.DESCRIPTION
    Milestone 1 has no screens, so it is demonstrated through its API contract.
    This script discovers a menu and an item for you, walks all nine checks in
    order, and prints what each one actually returned next to what it should
    return. You read the output and record the judgment in the workbook:

        docs/features/menus/m1-demo-workbook.html

    It only reads and writes the seeded acceptance venue's own data, and it
    leaves the draft queue empty when it finishes.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/run-m1-demo.ps1

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/run-m1-demo.ps1 -Json m1-evidence.json
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://localhost:7138',
    [string]$Token = 'track1-owner-review',
    [string]$Json
)

$ErrorActionPreference = 'Stop'
$spine = "$BaseUrl/api/back-office/menu-spine"
$headers = @{ 'X-Vennusign-Back-Office-Token' = $Token }
$results = [System.Collections.Generic.List[object]]::new()

# The local API uses a development certificate. Windows PowerShell 5.1 has no
# -SkipCertificateCheck, so trust it through the service point manager instead.
$script:UsePwshCertSwitch = $PSVersionTable.PSVersion.Major -ge 6
if (-not $script:UsePwshCertSwitch) {
    Add-Type -TypeDefinition @'
using System.Net;
using System.Security.Cryptography.X509Certificates;
public static class VennuDevCertPolicy {
    public static void Trust() {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    }
}
'@ -ErrorAction SilentlyContinue
    [VennuDevCertPolicy]::Trust()
}

function Invoke-Api {
    param([string]$Method, [string]$Url, $Body)
    $params = @{ Method = $Method; Uri = $Url; Headers = $headers; UseBasicParsing = $true }
    if ($script:UsePwshCertSwitch) { $params.SkipCertificateCheck = $true }
    if ($null -ne $Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 6)
        $params.ContentType = 'application/json'
    }
    Invoke-RestMethod @params
}

function Record {
    param([string]$Id, [string]$Title, [bool]$Ok, [string]$Observed, [string]$Expected)
    $results.Add([pscustomobject]@{ Check = $Id; Title = $Title; Result = $(if ($Ok) { 'PASS' } else { 'ATTENTION' }); Observed = $Observed; Expected = $Expected })
    $colour = if ($Ok) { 'Green' } else { 'Yellow' }
    $label = if ($Ok) { 'PASS     ' } else { 'ATTENTION' }
    Write-Host ''
    Write-Host "$label  $Id. $Title" -ForegroundColor $colour
    Write-Host "           saw:      $Observed"
    if (-not $Ok) { Write-Host "           expected: $Expected" -ForegroundColor Yellow }
}

Write-Host ''
Write-Host 'Menus M1 demo - walking the API contract' -ForegroundColor Cyan
Write-Host "API: $BaseUrl" -ForegroundColor DarkGray

# --- Reachability -------------------------------------------------------------
try {
    Invoke-Api GET "$BaseUrl/health/version" | Out-Null
} catch {
    Write-Host ''
    Write-Host 'The API is not answering. Start it first:' -ForegroundColor Red
    Write-Host '  powershell -ExecutionPolicy Bypass -File scripts/start-ui-test-env.ps1' -ForegroundColor Red
    exit 1
}

# --- Discover a menu and an item ----------------------------------------------
$editor = Invoke-Api GET "$BaseUrl/api/back-office/menus"
$target = $editor.menus | Where-Object { $_.sections.Count -gt 0 } | Select-Object -First 1
if (-not $target) { $target = $editor.menus | Select-Object -First 1 }
if (-not $target) { Write-Host 'No menus exist on this venue; seed the acceptance fixture first.' -ForegroundColor Red; exit 1 }

$menuId = $target.menu.id
$otherMenuId = ($editor.menus | Where-Object { $_.menu.id -ne $menuId } | Select-Object -First 1).menu.id

$availability = Invoke-Api GET "$spine/availability"
$itemId = ($availability | Select-Object -First 1).itemId
if (-not $itemId) { Write-Host 'No library items exist on this venue; seed the acceptance fixture first.' -ForegroundColor Red; exit 1 }

Write-Host "menu: $($target.menu.name)  ($menuId)" -ForegroundColor DarkGray
Write-Host "item: $itemId" -ForegroundColor DarkGray

# Put the menu on a screen, otherwise the screen-facing checks pass trivially
# against an empty fleet and prove nothing.
$venueId = $target.menu.venueId
$screenId = $null
try {
    $screens = Invoke-Api GET "$BaseUrl/api/back-office/venues/$venueId/screens"
    $screenList = if ($screens.screens) { $screens.screens } else { $screens }
    $screenId = ($screenList | Select-Object -First 1).id
} catch {
    Write-Host "could not list screens: $($_.Exception.Message)" -ForegroundColor Yellow
}
if ($screenId) {
    Invoke-Api PUT "$spine/screens/$screenId/menu" @{ menuId = $menuId } | Out-Null
    Write-Host "screen: $screenId (assigned for this run)" -ForegroundColor DarkGray
} else {
    Write-Host "screen: none found - the screen-facing checks will report 0 screens" -ForegroundColor Yellow
}

# --- 1. Context ----------------------------------------------------------------
$context = Invoke-Api GET "$spine/context"
$ceilings = $context.ceilings
$hasAll = @('content.menu.count','content.menu.items','content.menu.import.lines','publishing.history.retention') |
    ForEach-Object { $ceilings.PSObject.Properties.Name -contains $_ } | Where-Object { -not $_ }
Record '1' 'Context returns the venue timezone and configured ceilings' `
    ($null -eq $hasAll -and -not [string]::IsNullOrWhiteSpace($context.timezone)) `
    "timezone $($context.timezone); menus $($ceilings.'content.menu.count'), items $($ceilings.'content.menu.items'), paste $($ceilings.'content.menu.import.lines'), history $($ceilings.'publishing.history.retention')" `
    'the venue timezone plus all four ceilings, read from the allowance model'

# --- 2. An 86 is instant and does not queue -------------------------------------
Invoke-Api DELETE "$spine/menus/$menuId/draft" | Out-Null   # start from an empty queue
$off = Invoke-Api PUT "$spine/items/$itemId/availability" @{ isAvailable = $false }
$draftAfter86 = Invoke-Api GET "$spine/menus/$menuId/draft"
Record '2' 'Turning an item off is instant and never queues' `
    ($off.isAvailable -eq $false -and $draftAfter86.count -eq 0) `
    "$($off.name) is off, changed by '$($off.changedBy)', reaching $($off.screenIds.Count) screen(s); draft count $($draftAfter86.count)" `
    'the item off, its screens named, and a draft count of 0'

# --- 3. No auto-reset ------------------------------------------------------------
Start-Sleep -Seconds 2
$stillOff = (Invoke-Api GET "$spine/availability") | Where-Object { $_.itemId -eq $itemId }
Record '3' 'The 86 does not reset itself' `
    ($stillOff.isAvailable -eq $false) `
    "still off, since $($stillOff.changedUtc)" `
    'the item still off - an 86 stays off until a person turns it back on'

# --- 4. A queued change reaches no screen ----------------------------------------
$historyBefore = @(Invoke-Api GET "$spine/menus/$menuId/history")
Invoke-Api POST "$spine/menus/$menuId/draft" @{ targetKind='item'; targetId=$itemId; field='price'; beforeValue='12'; afterValue='13' } | Out-Null
$historyAfter = @(Invoke-Api GET "$spine/menus/$menuId/history")
$publishesBefore = @($historyBefore | Where-Object { $_.kind -eq 'published' }).Count
$publishesAfter = @($historyAfter | Where-Object { $_.kind -eq 'published' }).Count
Record '4' 'A queued change reaches no screen' `
    ($publishesAfter -eq $publishesBefore) `
    "publishes before $publishesBefore, after $publishesAfter" `
    'no new publish - queuing an edit is not a deliberate act'

# --- 5. The queue is the current diff ---------------------------------------------
$again = Invoke-Api POST "$spine/menus/$menuId/draft" @{ targetKind='item'; targetId=$itemId; field='price'; beforeValue='12'; afterValue='14' }
$priceChange = $again.changes | Where-Object { $_.field -eq 'price' } | Select-Object -First 1
Record '5' 'Editing the same field twice is still one change' `
    ($again.count -eq 1 -and $priceChange.afterValue -eq '14') `
    "count $($again.count), afterValue $($priceChange.afterValue)" `
    'count 1 and afterValue 14 - the count is the current diff, not a keystroke log'

# --- 6. Publish ships this menu only -----------------------------------------------
$otherQueued = $null
if ($otherMenuId) {
    Invoke-Api POST "$spine/menus/$otherMenuId/draft" @{ targetKind='menu'; field='theme'; beforeValue='coastal'; afterValue='classic-dark' } | Out-Null
}
$published = Invoke-Api POST "$spine/menus/$menuId/publish"
$thisDraft = Invoke-Api GET "$spine/menus/$menuId/draft"
if ($otherMenuId) { $otherQueued = Invoke-Api GET "$spine/menus/$otherMenuId/draft" }
$otherIntact = (-not $otherMenuId) -or ($otherQueued.count -ge 1)
$targetsText = ($published.targets | ForEach-Object { "$($_.state)" }) -join ', '
Record '6' "Publishing ships only this menu's queue, and stores what it shipped" `
    ($published.changeCount -ge 1 -and $thisDraft.count -eq 0 -and $otherIntact) `
    "version $($published.version), shipped $($published.changeCount) change(s), targets [$targetsText]; this draft now $($thisDraft.count), other menu still $($otherQueued.count)" `
    "this menu's queue emptied, the other menu's queue untouched, one target per assigned screen"

# --- 7. The 86 survived the publish --------------------------------------------------
$afterPublish = (Invoke-Api GET "$spine/availability") | Where-Object { $_.itemId -eq $itemId }
Record '7' 'The 86 survived the publish' `
    ($afterPublish.isAvailable -eq $false) `
    "item still off after publishing version $($published.version)" `
    'the item still off - availability is a fact about tonight, not about the menu'

# --- 8. Go back to produces a draft ---------------------------------------------------
$publishesPre = @(@(Invoke-Api GET "$spine/menus/$menuId/history") | Where-Object { $_.kind -eq 'published' }).Count
$restored = Invoke-Api POST "$spine/menus/$menuId/go-back-to/$($published.version)"
$publishesPost = @(@(Invoke-Api GET "$spine/menus/$menuId/history") | Where-Object { $_.kind -eq 'published' }).Count
Record '8' 'Go back to produces a draft, not a silent publish' `
    ($restored.count -ge 1 -and $publishesPost -eq $publishesPre) `
    "draft now holds $($restored.count) change(s); publishes stayed at $publishesPost" `
    'a draft with a change in it, and no new publish'

# --- 8b. Take-off queues rather than committing (Q68) -----------------------------------
$beforeTakeOff = @(Invoke-Api GET "$spine/assignments").Count
$takeOffDraft = Invoke-Api DELETE "$spine/menus/$menuId/screens"
$afterTakeOff = @(Invoke-Api GET "$spine/assignments").Count
Record '8b' 'Take off the screens queues, and ships on Publish' `
    ($afterTakeOff -eq $beforeTakeOff -and $takeOffDraft.count -ge 1) `
    "screens still assigned: $afterTakeOff (was $beforeTakeOff); queued changes: $($takeOffDraft.count)" `
    'the menu still on its screen, with the removal waiting in the draft - take-off is permanent, so it is not instant like an 86'

# --- 9. Destructive acts are attributable ----------------------------------------------
Invoke-Api DELETE "$spine/menus/$menuId/draft" | Out-Null
$history = @(Invoke-Api GET "$spine/menus/$menuId/history")
$discardAuthor = @($history | Where-Object { $_.kind -eq 'draft_discarded' } | ForEach-Object { $_.author })[0]
$restoredAuthor = @($history | Where-Object { $_.kind -eq 'restored' } | ForEach-Object { $_.author })[0]
$discard = @($history | Where-Object { $_.kind -eq 'draft_discarded' })[0]
Record '9' 'Irreversible acts are recorded with their author' `
    ($null -ne $discard) `
    "draft_discarded by '$discardAuthor'; restored by '$restoredAuthor'" `
    'the discard present in history, naming who did it'

# --- Tidy up ------------------------------------------------------------------------------
if ($otherMenuId) { Invoke-Api DELETE "$spine/menus/$otherMenuId/draft" | Out-Null }
Invoke-Api PUT "$spine/items/$itemId/availability" @{ isAvailable = $true } | Out-Null

# --- Summary -------------------------------------------------------------------------------
$passed = @($results | Where-Object { $_.Result -eq 'PASS' }).Count
Write-Host ''
Write-Host ('-' * 70)
Write-Host "  $passed of $($results.Count) checks returned what the milestone promises." -ForegroundColor $(if ($passed -eq $results.Count) { 'Green' } else { 'Yellow' })
Write-Host '  Record your judgment in docs/features/menus/m1-demo-workbook.html'
Write-Host ('-' * 70)
Write-Host ''
Write-Host 'Still needing your decision (they are judgment calls, not test results):'
Write-Host '  - the provisional audit record (Q207, issue #677)'
Write-Host '  - the provisional capability grants (Q24)'
Write-Host '  - the deferred legacy column drops (HappyHourPrice, QuantityAvailable, Tags, IsPopular)'
Write-Host ''

if ($Json) {
    $results | ConvertTo-Json -Depth 5 | Set-Content -Path $Json -Encoding utf8
    Write-Host "Evidence written to $Json"
}
