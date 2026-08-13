<#
.SYNOPSIS
    Menus milestone 3, step 2 gate: the builder's content API over real HTTP.

.DESCRIPTION
    Exercises everything the four-column builder is built on - the working-board
    read, section add/rename/delete/reorder, placing and removing items, editing an
    item's shared values, the add-row search, and the (empty) theme list - as real
    requests against a running API on a real database, before any UI consumes them.

    No test double anywhere in here, on purpose. These rules are decided inside the
    statements that write them - the next sort order under a lock, the ceiling under
    a lock, "already on this board", and whether a reorder list still matches the
    menu - so an in-memory stand-in would prove the copy, and the copy is what
    drifted in milestone 1.

    Start the environment first:  scripts\start-ui-test-env.ps1
    Then run this. It cleans up the menu it creates.
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://localhost:7138',
    [string]$Token = 'track1-owner-review'
)

$ErrorActionPreference = 'Stop'
$base = "$BaseUrl/api/back-office/content"
$headers = @{ 'X-Vennusign-Back-Office-Token' = $Token }
$fails = 0
$checks = 0

# Windows PowerShell 5.1 has no -SkipCertificateCheck; trust the dev certificate
# through the service point manager instead, exactly as run-m1-demo.ps1 does.
$script:UsePwshCertSwitch = $PSVersionTable.PSVersion.Major -ge 6
if (-not $script:UsePwshCertSwitch) {
    Add-Type -TypeDefinition @'
using System.Net;
public static class VennuM3CertPolicy {
    public static void Trust() {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    }
}
'@ -ErrorAction SilentlyContinue
    [VennuM3CertPolicy]::Trust()
}

function Invoke-Api {
    param([string]$Method, [string]$Path, $Body)
    $params = @{ Method = $Method; Uri = "$base$Path"; Headers = $headers; UseBasicParsing = $true }
    if ($script:UsePwshCertSwitch) { $params.SkipCertificateCheck = $true }
    if ($null -ne $Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 6)
        $params.ContentType = 'application/json'
    }
    return Invoke-RestMethod @params
}

# PowerShell 5.1/7 both hand back a single JSON array as one object whose shape
# varies with row count. Everything list-shaped goes through this.
function Expand-Api { param($Response) if ($null -eq $Response) { return @() } return @($Response) }

function Get-Status {
    param([scriptblock]$Call)
    try { & $Call | Out-Null; return 200 }
    catch { return $_.Exception.Response.StatusCode.value__ }
}

function Assert-That {
    param([string]$Name, [bool]$Condition, [string]$Saw)
    $script:checks++
    if ($Condition) { Write-Host ("PASS  {0}" -f $Name) -ForegroundColor Green }
    else { $script:fails++; Write-Host ("FAIL  {0}" -f $Name) -ForegroundColor Red }
    Write-Host ("      saw: {0}" -f $Saw)
}

Write-Host "`nMilestone 3 step 2 gate - the builder's API over real HTTP`n" -ForegroundColor Cyan

# ---- a menu of our own, so nothing here reads another spec's rows -------------
$suffix = (Get-Random -Maximum 99999)
$createParams = @{
    Method          = 'Post'
    Uri             = "$BaseUrl/api/back-office/menus"
    Headers         = $headers
    Body            = (@{ name = "m3 gate $suffix" } | ConvertTo-Json)
    ContentType     = 'application/json'
    UseBasicParsing = $true
}
if ($script:UsePwshCertSwitch) { $createParams.SkipCertificateCheck = $true }
$menu = Invoke-RestMethod @createParams
$menuId = $menu.id

try {
    # ---- 1. Sections ---------------------------------------------------------
    $starters = Invoke-Api POST "/menus/$menuId/sections" @{ name = 'Starters' }
    $mains = Invoke-Api POST "/menus/$menuId/sections" @{ name = 'Mains' }

    Assert-That 'A new section lands at the end of the board' `
        ($starters.sortOrder -eq 0 -and $mains.sortOrder -eq 1) `
        ("Starters at $($starters.sortOrder), Mains at $($mains.sortOrder)")

    $blank = Get-Status { Invoke-Api POST "/menus/$menuId/sections" @{ name = '   ' } }
    Assert-That 'A blank section name is refused, not saved' ($blank -eq 400) ("HTTP $blank")

    Invoke-Api PUT "/menus/$menuId/sections/$($starters.sectionId)" @{ name = 'Small Plates' } | Out-Null
    $board = Invoke-Api GET "/menus/$menuId/board"
    $renamed = @($board.board.sections | Where-Object { $_.sectionId -eq $starters.sectionId })[0]
    Assert-That 'Renaming a section is what the canvas heading does' ($renamed.name -eq 'Small Plates') `
        ("heading reads '$($renamed.name)'")

    # ---- 2. The working board is what the canvas draws -----------------------
    $place = Invoke-Api POST "/menus/$menuId/sections/$($starters.sectionId)/items" @{ name = 'Harbor Lemonade' }
    Assert-That 'Create-as-new is born with the typed name and no price' `
        ($place.outcome -eq 'placed' -and $null -ne $place.itemId) `
        ("outcome $($place.outcome), item $($place.itemId)")

    $board = Invoke-Api GET "/menus/$menuId/board"
    $item = @(@($board.board.sections | Where-Object { $_.sectionId -eq $starters.sectionId })[0].items)[0]
    Assert-That 'A missing price is a blank, never a zero' ($null -eq $item.price) ("price: '$($item.price)'")
    Assert-That 'The board read carries the draft it differs from its screens by' `
        ($board.draftCount -gt 0) ("$($board.draftCount) change(s) waiting")

    # ---- 3. One item, one shared price ---------------------------------------
    Invoke-Api PUT "/items/$($place.itemId)" @{ name = 'Harbor Lemonade'; description = 'Over crushed ice.'; price = '9.5' } | Out-Null
    $board = Invoke-Api GET "/menus/$menuId/board"
    $item = @(@($board.board.sections | Where-Object { $_.sectionId -eq $starters.sectionId })[0].items)[0]
    Assert-That 'A price round-trips exactly as typed' ($item.price -eq '9.5') ("price '$($item.price)'")

    Invoke-Api PUT "/items/$($place.itemId)" @{ name = 'Harbor Lemonade'; description = $null; price = 'MP' } | Out-Null
    $board = Invoke-Api GET "/menus/$menuId/board"
    $item = @(@($board.board.sections | Where-Object { $_.sectionId -eq $starters.sectionId })[0].items)[0]
    Assert-That 'MP is a price, not a number that failed to parse' ($item.price -eq 'MP') ("price '$($item.price)'")

    # An emptied name reverts rather than saving blank (Q119).
    Invoke-Api PUT "/items/$($place.itemId)" @{ name = '  '; description = $null; price = 'MP' } | Out-Null
    $board = Invoke-Api GET "/menus/$menuId/board"
    $item = @(@($board.board.sections | Where-Object { $_.sectionId -eq $starters.sectionId })[0].items)[0]
    Assert-That 'An emptied name reverts instead of saving blank' ($item.name -eq 'Harbor Lemonade') `
        ("name '$($item.name)'")

    # ---- 4. Q112: already on this board jumps, never duplicates ---------------
    $again = Invoke-Api POST "/menus/$menuId/sections/$($mains.sectionId)/items" @{ itemId = $place.itemId }
    Assert-That 'Placing an item already on this board says where it is' `
        ($again.outcome -eq 'already_on_board' -and $again.sectionId -eq $starters.sectionId) `
        ("outcome $($again.outcome), sits in section $($again.sectionId)")

    $board = Invoke-Api GET "/menus/$menuId/board"
    $count = @($board.board.sections | ForEach-Object { $_.items }).Count
    Assert-That 'And places nothing' ($count -eq 1) ("$count item(s) on the board")

    # ---- 5. The add row's search ---------------------------------------------
    $hits = Expand-Api (Invoke-Api GET '/items?query=Harbor&take=20')
    $found = @($hits | Where-Object { $_.itemId -eq $place.itemId })
    Assert-That 'The add row searches the whole venue library' ($found.Count -eq 1) `
        ("$($hits.Count) hit(s), ours included")
    Assert-That 'Each result names the boards it already sits on' `
        (@($found[0].boards | Where-Object { $_.menuId -eq $menuId }).Count -eq 1) `
        ("on: " + ((@($found[0].boards) | ForEach-Object { $_.menuName }) -join ', '))

    $literal = Expand-Api (Invoke-Api GET '/items?query=%25&take=50')
    Assert-That 'A typed wildcard is text, not an operator' ($literal.Count -eq 0) `
        ("$($literal.Count) hit(s) for a literal percent sign")

    # ---- 6. Reorder refuses a list that no longer matches ---------------------
    $order = Get-Status { Invoke-Api PUT "/menus/$menuId/sections/order" @{ sectionIds = @($mains.sectionId) } }
    Assert-That 'A stale reorder is refused whole, not half-applied' ($order -eq 409) ("HTTP $order")

    Invoke-Api PUT "/menus/$menuId/sections/order" @{ sectionIds = @($mains.sectionId, $starters.sectionId) } | Out-Null
    $board = Invoke-Api GET "/menus/$menuId/board"
    Assert-That 'A complete reorder swaps them without colliding on the unique order' `
        (@($board.board.sections)[0].sectionId -eq $mains.sectionId) `
        ("first section is now '$(@($board.board.sections)[0].name)'")

    # ---- 7. Removing, and deleting a section ---------------------------------
    Invoke-Api DELETE "/menus/$menuId/pages/$($board.board.pages[0].pageId)/items/$($place.itemId)" | Out-Null
    $stillInLibrary = @(Expand-Api (Invoke-Api GET '/items?query=Harbor&take=20') | Where-Object { $_.itemId -eq $place.itemId })
    Assert-That 'Removing an item from a board leaves it in the library' ($stillInLibrary.Count -eq 1) `
        ("library still holds it: $($stillInLibrary.Count)")

    $replaced = Invoke-Api POST "/menus/$menuId/sections/$($starters.sectionId)/items" @{ itemId = $place.itemId }
    $deleted = Invoke-Api DELETE "/menus/$menuId/sections/$($starters.sectionId)"
    Assert-That 'Deleting a section says how many items it released' ($deleted.releasedItemCount -eq 1) `
        ("released $($deleted.releasedItemCount)")
    $afterDelete = @(Expand-Api (Invoke-Api GET '/items?query=Harbor&take=20') | Where-Object { $_.itemId -eq $place.itemId })
    Assert-That 'And the released item is still in the library' ($afterDelete.Count -eq 1) `
        ("library still holds it: $($afterDelete.Count)")

    # ---- 8. Themes: an honest empty list --------------------------------------
    $themes = Expand-Api (Invoke-Api GET '/menu-themes')
    Assert-That 'The theme picker reads an empty list rather than a hard-coded one' ($themes.Count -eq 0) `
        ("$($themes.Count) theme(s)")

    # ---- 9. Nothing here reached a screen -------------------------------------
    $showing = Expand-Api (Invoke-Api GET '/screens/showing')
    $ours = @($showing | Where-Object { $_.menuId -eq $menuId })
    Assert-That 'Not one of these edits reached a screen without a publish' ($ours.Count -eq 0) `
        ("$($ours.Count) screen(s) showing this menu")
}
finally {
    try { Invoke-Api PUT "/menus/$menuId/put-away" @{ isPutAway = $true } | Out-Null } catch { }
}

Write-Host ""
if ($fails -eq 0) {
    Write-Host ("Step 2 gate: {0} of {0} checks passed." -f $checks) -ForegroundColor Green
    exit 0
}

Write-Host ("Step 2 gate: {0} of {1} checks FAILED." -f $fails, $checks) -ForegroundColor Red
exit 1
