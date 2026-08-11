<#
.SYNOPSIS
    Runs every Menus M1 demo check against a local API and prints the evidence.

.DESCRIPTION
    Milestone 1 has no screens, so it is demonstrated through its API contract.
    The save model is the derived draft: edits change the menu immediately, the
    screens keep showing the last published snapshot, and the draft is the
    computed difference between the two. This script makes real edits through
    the editor endpoints, watches the derived draft respond, walks all the
    checks in order, and prints what each one actually returned. You read the
    output and record the judgment in the workbook:

        docs/features/menus/m1-demo-workbook.html

    It only reads and writes the seeded acceptance venue's own data. Re-running
    scripts/start-ui-test-env.ps1 restores the canonical fixture state.

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
$content = "$BaseUrl/api/back-office/content"
$menus = "$BaseUrl/api/back-office/menus"
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

# Windows PowerShell 5.1 does not put a JSON array onto the pipeline row by row: it
# emits the whole array as one object. Read a property off that and PowerShell
# member-enumerates every row at once, and -eq against the resulting array *filters*
# instead of comparing - so a Where-Object over it passes every row through and the
# check silently proves nothing.
#
# Worse, the shape depends on the row count: one row arrives unwrapped, many do not,
# and an empty array arrives as $null while @($null).Count is 1. So a reader that
# looks correct against one row starts lying the day the data grows. Every list read
# in this script goes through here.
function Expand-Api {
    param($Response)
    $rows = New-Object System.Collections.ArrayList
    foreach ($item in @($Response)) {
        if ($null -eq $item) { continue }
        if ($item -is [System.Collections.IEnumerable] -and $item -isnot [string]) {
            foreach ($inner in $item) { if ($null -ne $inner) { [void]$rows.Add($inner) } }
        }
        else { [void]$rows.Add($item) }
    }
    $rows.ToArray()
}

function Measure-Api {
    param([string]$Url)
    @(Expand-Api (Invoke-Api GET $Url)).Count
}

# What a screen is actually showing, from the published side. The whole model rests
# on "a screen shows the last published version and only a publish changes that", so
# the demo asks the screen rather than asking the API whether it accepted a request.
# What each screen is actually showing, from the published side.
function Get-AllShowing {
    Expand-Api (Invoke-Api GET "$content/screens/showing")
}

function Get-Showing {
    param([string]$ScreenId)
    @(Get-AllShowing | Where-Object { $_.screenId -eq $ScreenId })[0]
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
Write-Host 'Menus M1 demo - walking the API contract of the derived draft' -ForegroundColor Cyan
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

# --- Discover a menu, a section, and a placed item ------------------------------
$editor = Invoke-Api GET $menus
$target = $editor.menus | Where-Object {
    $menuSections = $_.sections
    $editor.itemGroups | Where-Object { $_.sectionId -in $menuSections.id -and $_.items.Count -gt 0 }
} | Select-Object -First 1
if (-not $target) { Write-Host 'No menu with placed items exists; run start-ui-test-env.ps1 to seed the acceptance fixture.' -ForegroundColor Red; exit 1 }

$menuId = $target.menu.id
$venueId = $target.menu.venueId
$group = $editor.itemGroups | Where-Object { $_.sectionId -in $target.sections.id -and $_.items.Count -gt 0 } | Select-Object -First 1
$sectionId = $group.sectionId
$item = $group.items | Where-Object { $_.price -gt 0 } | Select-Object -First 1
if (-not $item) { $item = $group.items | Select-Object -First 1 }
$itemId = $item.id
$basePrice = [double]$item.price

Write-Host "menu: $($target.menu.name)  ($menuId)" -ForegroundColor DarkGray
Write-Host "item: $($item.name)  ($itemId), price $basePrice" -ForegroundColor DarkGray

function Set-ItemPrice {
    param([double]$Price)
    # The item write moved onto the content API with milestone 3: one item is one
    # shared set of values across every board it sits on, so it is addressed by
    # item rather than by where it happens to be placed.
    # A price is text, exactly as somebody typed it (Q115/Q190) - "9.5" never
    # becomes 9.50, and "MP" is a price. Sending a number here is what the old
    # per-placement route accepted and the content API rightly refuses.
    Invoke-Api PUT "$content/items/$itemId" @{ name = $item.name; description = $item.description; price = "$Price" }
}

# Put the menu on a screen, and prove it. Every screen-facing check below is
# meaningless without one, and passing them against an assignment that merely
# happened to be in the fixture already would make this demo state-dependent
# rather than self-proving. So it is established here and verified, or the run
# stops.
$screenId = $null
try {
    $screens = Invoke-Api GET "$BaseUrl/api/back-office/venues/$venueId/screens"
    # The endpoint answers with a bare array. Reading `.screens` off one would
    # return a null per element - a non-empty array, and therefore truthy - which
    # silently selected nothing as soon as the venue had more than one screen.
    $screenList = if ($null -ne $screens -and $screens.PSObject.Properties.Name -contains 'screens') { $screens.screens } else { $screens }
    $screenId = @($screenList | Where-Object { $_ -and $_.id } | Select-Object -First 1).id
} catch {
    Write-Host "could not list screens: $($_.Exception.Message)" -ForegroundColor Red
}
if (-not $screenId) {
    Write-Host ''
    Write-Host 'No screen exists on this venue, so the screen-facing checks would prove nothing.' -ForegroundColor Red
    Write-Host 'Seed the acceptance fixture first:' -ForegroundColor Red
    Write-Host '  powershell -ExecutionPolicy Bypass -File scripts/start-ui-test-env.ps1' -ForegroundColor Red
    exit 1
}

Invoke-Api PUT "$content/screens/$screenId/menu" @{ menuId = $menuId } | Out-Null
$assignedNow = @((Invoke-Api GET "$content/assignments") | Where-Object { $null -ne $_ -and $_.screenId -eq $screenId -and $_.menuId -eq $menuId })
if ($assignedNow.Count -ne 1) {
    Write-Host ''
    Write-Host "The demo could not put menu $menuId on screen $screenId, so its screen-facing checks would be meaningless." -ForegroundColor Red
    exit 1
}
Write-Host "screen: $screenId (assigned by this run, and verified)" -ForegroundColor DarkGray

# Publish once so the run starts from a known clean state: working = published.
Invoke-Api POST "$content/menus/$menuId/publish" | Out-Null

# --- 1. Context ----------------------------------------------------------------
$context = Invoke-Api GET "$content/context"
$ceilings = $context.ceilings
$hasAll = @('content.menu.count','content.menu.items','content.menu.import.lines','publishing.history.retention') |
    ForEach-Object { $ceilings.PSObject.Properties.Name -contains $_ } | Where-Object { -not $_ }
Record '1' 'Context returns the venue timezone and configured ceilings' `
    ($null -eq $hasAll -and -not [string]::IsNullOrWhiteSpace($context.timezone)) `
    "timezone $($context.timezone); menus $($ceilings.'content.menu.count'), items $($ceilings.'content.menu.items'), paste $($ceilings.'content.menu.import.lines'), history $($ceilings.'publishing.history.retention')" `
    'the venue timezone plus all four ceilings, read from the allowance model'

# --- 2. An 86 is instant and does not queue -------------------------------------
$off = Invoke-Api PUT "$content/items/$itemId/availability" @{ isAvailable = $false }
$draftAfter86 = Invoke-Api GET "$content/menus/$menuId/draft"
Record '2' 'Turning an item off is instant and never queues' `
    ($off.isAvailable -eq $false -and $draftAfter86.count -eq 0) `
    "$($off.name) is off, changed by '$($off.changedBy)', notification sent for $($off.screenIds.Count) screen(s); draft count $($draftAfter86.count)" `
    'the item off with a draft count of 0. Honest scope: this milestone proves the notification contract; the screens render the new model when the M4 player lands'

# --- 3. No auto-reset ------------------------------------------------------------
Start-Sleep -Seconds 2
$stillOff = (Invoke-Api GET "$content/availability") | Where-Object { $_.itemId -eq $itemId }
Record '3' 'The 86 does not reset itself' `
    ($stillOff.isAvailable -eq $false) `
    "still off, since $($stillOff.changedUtc)" `
    'the item still off - an 86 stays off until a person turns it back on'

# --- 4. An edit changes the menu now, and no screen until Publish -----------------
$historyBefore = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $null -ne $_ })
$versionBeforeEdit = (Get-Showing $screenId).version
Set-ItemPrice ($basePrice + 1) | Out-Null
$draftAfterEdit = Invoke-Api GET "$content/menus/$menuId/draft"
$historyAfter = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $null -ne $_ })
$publishesBefore = @($historyBefore | Where-Object { $_.kind -eq 'published' }).Count
$publishesAfter = @($historyAfter | Where-Object { $_.kind -eq 'published' }).Count
$priceChange = $draftAfterEdit.changes | Where-Object { $_.field -eq 'price' } | Select-Object -First 1
$showingAfterEdit = Get-Showing $screenId
$screenUnmovedByEdit = ($null -eq $showingAfterEdit.menuId) -or ($showingAfterEdit.version -eq $versionBeforeEdit)
Record '4' 'An edit shows up as a derived change and reaches no screen' `
    ($draftAfterEdit.count -eq 1 -and $null -ne $priceChange -and $publishesAfter -eq $publishesBefore -and $screenUnmovedByEdit) `
    "draft count $($draftAfterEdit.count), price $($priceChange.beforeValue) -> $($priceChange.afterValue); publishes before $publishesBefore, after $publishesAfter; the screen is still showing version $($showingAfterEdit.version)" `
    'one derived change with the before-value taken from the published snapshot, no new publish - and the screen still showing exactly what it showed before the edit'

# --- 5. The count is the current diff ---------------------------------------------
Set-ItemPrice ($basePrice + 2) | Out-Null
$again = Invoke-Api GET "$content/menus/$menuId/draft"
$secondChange = $again.changes | Where-Object { $_.field -eq 'price' } | Select-Object -First 1
Set-ItemPrice $basePrice | Out-Null
$reverted = Invoke-Api GET "$content/menus/$menuId/draft"
Record '5' 'Editing twice is one change, and an edit taken back is none' `
    ($again.count -eq 1 -and $secondChange.afterValue -ne $priceChange.afterValue -and $reverted.count -eq 0) `
    "after a second edit: count $($again.count), afterValue $($secondChange.afterValue); after typing the published price back in: count $($reverted.count)" `
    'count 1 with the latest value while different, count 0 once the price matches the screens again - the count is the diff, not a keystroke log'

# --- 6. Publish ships this menu only -----------------------------------------------
Set-ItemPrice ($basePrice + 3) | Out-Null
$otherMenuName = 'M1 demo holding shelf'
$otherMenuId = ($editor.menus | Where-Object { $_.menu.name -eq $otherMenuName } | Select-Object -First 1).menu.id
if (-not $otherMenuId) {
    try { $otherMenuId = (Invoke-Api POST $menus @{ name = $otherMenuName }).id } catch { $otherMenuId = $null }
}
$otherBefore = if ($otherMenuId) { (Invoke-Api GET "$content/menus/$otherMenuId/draft").count } else { $null }
$published = Invoke-Api POST "$content/menus/$menuId/publish"
$thisDraft = Invoke-Api GET "$content/menus/$menuId/draft"
$otherAfter = if ($otherMenuId) { (Invoke-Api GET "$content/menus/$otherMenuId/draft").count } else { $null }
$otherIntact = (-not $otherMenuId) -or ($otherAfter -eq $otherBefore -and $otherAfter -ge 1)
$targetsText = ($published.targets | ForEach-Object { "$($_.state)" }) -join ', '
$showingAfterPublish = Get-Showing $screenId
$screenGotThePublish = ($showingAfterPublish.menuId -eq $menuId -and $showingAfterPublish.version -eq $published.version)
Record '6' "Publishing ships this menu's diff and no other menu's" `
    ($published.changeCount -ge 1 -and $thisDraft.count -eq 0 -and $otherIntact -and $screenGotThePublish) `
    "version $($published.version), shipped $($published.changeCount) change(s), targets [$targetsText]; this draft now $($thisDraft.count), the never-published menu still waits with $otherAfter" `
    "this menu's diff shipped and emptied, the other menu's pending content untouched, one target per assigned screen"

# --- 7. The 86 survived the publish --------------------------------------------------
$afterPublish = (Invoke-Api GET "$content/availability") | Where-Object { $_.itemId -eq $itemId }
Record '7' 'The 86 survived the publish' `
    ($afterPublish.isAvailable -eq $false) `
    "item still off after publishing version $($published.version)" `
    'the item still off - availability is a fact about tonight, not about the menu'

# --- 8. Go back to produces a draft ---------------------------------------------------
$publishesPre = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $_.kind -eq 'published' }).Count
$restored = Invoke-Api POST "$content/menus/$menuId/go-back-to/$($published.version - 1)"
$publishesPost = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $_.kind -eq 'published' }).Count
Record '8' 'Go back to produces a draft, not a silent publish' `
    ($restored.count -ge 1 -and $publishesPost -eq $publishesPre) `
    "back to version $($published.version - 1): draft now holds $($restored.count) change(s); publishes stayed at $publishesPost" `
    'the older values waiting as a draft against the current screens, and no new publish'

# --- 8b. Take-off queues rather than committing (Q68) -----------------------------------
$beforeTakeOff = Measure-Api "$content/assignments"
$publishesBeforeTakeOff = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $_.kind -eq 'published' }).Count
$takeOffDraft = Invoke-Api DELETE "$content/menus/$menuId/screens"
$afterTakeOff = Measure-Api "$content/assignments"
$publishesAfterTakeOff = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $_.kind -eq 'published' }).Count
$screensChange = $takeOffDraft.changes | Where-Object { $_.targetKind -eq 'screens' } | Select-Object -First 1
Record '8b' 'Take off the screens waits in the draft, and ships on Publish' `
    ($null -ne $screensChange -and $takeOffDraft.count -ge 1 -and $publishesAfterTakeOff -eq $publishesBeforeTakeOff) `
    "screens change '$($screensChange.beforeValue)' -> '$($screensChange.afterValue)' waiting in a draft of $($takeOffDraft.count); working assignments now $afterTakeOff (was $beforeTakeOff); publishes stayed at $publishesAfterTakeOff" `
    'the removal waiting as a screens change with no new publish - the menu leaves the working state now, and leaves the screens on the next Publish (Q68)'

# --- 8c. Take-off ships on Publish, and is recorded again against it ---------------------
# The screen is still showing the menu until this publish carries the take-off, so
# putting it away now would strand that screen: the act is refused until it ships.
$putAwayTooEarlyRefused = $false
try { Invoke-Api PUT "$content/menus/$menuId/put-away" @{ isPutAway = $true } | Out-Null } catch { $putAwayTooEarlyRefused = $true }

$takeOffPublish = Invoke-Api POST "$content/menus/$menuId/publish"
$historyAfterShip = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $null -ne $_ })
$takeOffEntries = @($historyAfterShip | Where-Object { $_.kind -eq 'taken_off_screens' })
# This menu's assignments, not the venue's. A whole-venue count was true on a
# clean fixture and stopped being true the moment anything else in the repo
# assigned a screen - which made a green check depend on what else had run.
$assignmentsAfterShip = @(Expand-Api (Invoke-Api GET "$content/assignments") | Where-Object { $_.menuId -eq $menuId }).Count
$shippedSnapshotScreens = @($takeOffPublish.targets).Count
$showingAfterTakeOff = Get-Showing $screenId
Record '8c' 'Publishing the take-off reaches the screens it is leaving, and records the act' `
    ($takeOffEntries.Count -ge 2 -and $assignmentsAfterShip -eq 0 -and $putAwayTooEarlyRefused -and $null -eq $showingAfterTakeOff.menuId) `
    "putting it away before the take-off shipped was refused: $putAwayTooEarlyRefused; taken_off_screens recorded $($takeOffEntries.Count) time(s) - when it was done and when it shipped, by '$(@($takeOffEntries | ForEach-Object { $_.author })[0])'; the publish told $shippedSnapshotScreens screen(s) it is being released; assignments now $assignmentsAfterShip; the screen is now showing nothing" `
    'the released screen told by the publish that releases it, the act attributable both when queued and when shipped, and no way to shelve the menu while a screen is still showing it'

# --- 8d. Put away is deliberate, attributable, and frees ceiling room ---------------------
$contextBeforePutAway = Invoke-Api GET "$content/context"
$putAway = Invoke-Api PUT "$content/menus/$menuId/put-away" @{ isPutAway = $true }
$contextAfterPutAway = Invoke-Api GET "$content/context"
$putAwayEntry = @(@((Invoke-Api GET "$content/menus/$menuId/history")) | Where-Object { $_.kind -eq 'put_away' })[0]

# A put-away menu is off the shelf: putting it back is the one way on, so
# assigning it a screen and publishing it must both refuse.
$assignRefused = $false
try { Invoke-Api PUT "$content/screens/$screenId/menu" @{ menuId = $menuId } | Out-Null } catch { $assignRefused = $true }
$publishRefused = $false
try { Invoke-Api POST "$content/menus/$menuId/publish" | Out-Null } catch { $publishRefused = $true }

# The state the model says cannot exist: put away, and still on a screen.
$screensStillShowingIt = @(Get-AllShowing | Where-Object { $_.menuId -eq $menuId }).Count
$shelvedShowsNowhere = $screensStillShowingIt -eq 0

Record '8d' 'Put away is attributable, frees ceiling room, and is the only way back on the shelf' `
    ($putAway.changed -eq $true -and $null -ne $putAwayEntry -and $contextAfterPutAway.menuCount -lt $contextBeforePutAway.menuCount -and $assignRefused -and $publishRefused -and $shelvedShowsNowhere) `
    "put away by '$($putAwayEntry.author)'; active menus $($contextBeforePutAway.menuCount) -> $($contextAfterPutAway.menuCount); giving it a screen refused: $assignRefused; publishing it refused: $publishRefused; screens showing this menu: $screensStillShowingIt" `
    "the act recorded with its author, the count dropping - the refusal says 'put one away first', so putting one away has to make room - neither assigning nor publishing able to put it back quietly, and no screen anywhere still showing it"

# Put it back so the venue is left as it was found.
Invoke-Api PUT "$content/menus/$menuId/put-away" @{ isPutAway = $false } | Out-Null

# --- 9. Destructive acts are attributable ----------------------------------------------
Invoke-Api DELETE "$content/menus/$menuId/draft" | Out-Null
$history = @((Invoke-Api GET "$content/menus/$menuId/history") | Where-Object { $null -ne $_ })
$kinds = @($history | ForEach-Object { $_.kind } | Sort-Object -Unique)
$anonymous = @($history | Where-Object { [string]::IsNullOrWhiteSpace($_.author) })
$missingKinds = @(@('taken_off_screens','put_away','put_back','restored') | Where-Object { $kinds -notcontains $_ })
Record '9' 'Every irreversible act is recorded with its author' `
    ($missingKinds.Count -eq 0 -and $anonymous.Count -eq 0) `
    "history holds: $($kinds -join ', '); entries with no author: $($anonymous.Count)" `
    'take-off, put away, put back and restore all present, each naming who did it - nothing irreversible is anonymous'

# --- Tidy up ------------------------------------------------------------------------------
Invoke-Api PUT "$content/items/$itemId/availability" @{ isAvailable = $true } | Out-Null
if ($screenId) { Invoke-Api PUT "$content/screens/$screenId/menu" @{ menuId = $menuId } | Out-Null }

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
