<#
.SYNOPSIS
    Menus milestone 2, step 2 gate: the content API's shelf additions over real HTTP.

.DESCRIPTION
    Exercises the four additions the Menus home shelf is built on - the shelf read,
    the published-board read, the version history carries, and duplicate - as real
    requests against a running API on a real database, before any UI consumes them.

    There is no test double anywhere in here on purpose. These rules are enforced in
    SQL and answered over HTTP, so this asserts them where they are enforced; an
    in-memory stand-in would prove the copy, and the copy is what drifted in
    milestone 1.

    Start the environment first:  scripts\start-ui-test-env.ps1
    Then run this. It cleans up the menus it creates.
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
public static class VennuStep2CertPolicy {
    public static void Trust() {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    }
}
'@ -ErrorAction SilentlyContinue
    [VennuStep2CertPolicy]::Trust()
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

function Assert-That {
    param([string]$Name, [bool]$Condition, [string]$Saw)
    $script:checks++
    if ($Condition) { Write-Host ("PASS  {0}" -f $Name) -ForegroundColor Green }
    else { $script:fails++; Write-Host ("FAIL  {0}" -f $Name) -ForegroundColor Red }
    Write-Host ("      saw: {0}" -f $Saw)
}

Write-Host "`nStep 2 gate - the four additions over real HTTP`n" -ForegroundColor Cyan

# ---- 1. The shelf read -------------------------------------------------------
$shelf = Expand-Api (Invoke-Api GET '/menus')
Assert-That 'GET menus returns every menu in the venue' ($shelf.Count -gt 0) `
    ("$($shelf.Count) menu(s)")

$withBoard = @($shelf | Where-Object { $null -ne $_.board })
Assert-That 'A published menu carries the board its screens are showing' ($withBoard.Count -gt 0) `
    ("$($withBoard.Count) of $($shelf.Count) card(s) have a published board")

$card = $withBoard[0]
Assert-That 'The card names the version, time and author that published it' `
    ($null -ne $card.publishedVersion -and $null -ne $card.lastPublishedUtc) `
    ("version $($card.publishedVersion), at $($card.lastPublishedUtc), by '$($card.lastPublishedBy)'")

$neverPublished = @($shelf | Where-Object { $null -eq $_.board })
Assert-That 'A never-published menu is a card with no board, not an error' ($neverPublished.Count -gt 0) `
    ("$($neverPublished.Count) card(s) have never been published")

Assert-That 'No card claims a theme that does not exist' `
    (@($shelf | Where-Object { $null -ne $_.theme }).Count -eq 0) `
    ("themes attached: " + (@($shelf | Where-Object { $null -ne $_.theme }).Count))

# The count and the board on one card describe the same pair of snapshots.
$menuId = $card.menuId
$draft = Invoke-Api GET "/menus/$menuId/draft"
Assert-That 'The card count is the same difference the draft endpoint reports' `
    ($card.draftCount -eq $draft.count) `
    ("card says $($card.draftCount), draft says $($draft.count)")

# ---- 2. The published-board read ---------------------------------------------
$board = Invoke-Api GET "/menus/$menuId/published-board"
Assert-That 'GET published-board returns the board with its own version' `
    ($board.version -eq $card.publishedVersion -and $null -ne $board.board) `
    ("version $($board.version), $(@($board.board.sections).Count) section(s)")

$prices = @($board.board.sections | ForEach-Object { $_.items } | ForEach-Object { $_.price })
Assert-That 'Prices come back exactly as typed' `
    ($prices.Count -eq 0 -or ($prices | Where-Object { $_ -is [string] }).Count -eq $prices.Count) `
    ("prices: " + ($prices -join ', '))

$missing = $false
try { Invoke-Api GET "/menus/$($neverPublished[0].menuId)/published-board" | Out-Null }
catch { $missing = $_.Exception.Response.StatusCode.value__ -eq 404 }
Assert-That 'A never-published menu answers 404, not an empty board' $missing `
    ("404 for menu $($neverPublished[0].menuId)")

# ---- 3. Version on history ----------------------------------------------------
$history = Expand-Api (Invoke-Api GET "/menus/$menuId/history")
$published = @($history | Where-Object { $_.kind -eq 'published' })
Assert-That 'A publish entry carries the version it published' `
    ($published.Count -gt 0 -and $null -ne $published[0].version) `
    ("$($published.Count) publish entries, newest is version $($published[0].version)")

$nonPublish = @($history | Where-Object { $_.kind -ne 'published' -and $null -ne $_.version })
Assert-That 'Entries that are not a publish borrow no version' ($nonPublish.Count -eq 0) `
    ("$($nonPublish.Count) non-publish entries carry a version")

# Go back to, addressed by a version learned from history alone.
$target = $published | Sort-Object version | Select-Object -First 1
$restored = Invoke-Api POST "/menus/$menuId/go-back-to/$($target.version)"
Assert-That 'Go back to is reachable using only a version read from history' `
    ($null -ne $restored) `
    ("went back to version $($target.version); draft now holds $($restored.count) change(s)")
Invoke-Api DELETE "/menus/$menuId/draft" | Out-Null

# ---- 4. Duplicate --------------------------------------------------------------
$before = Expand-Api (Invoke-Api GET '/menus')
$copy = Invoke-Api POST "/menus/$menuId/duplicate"
Assert-That 'Duplicate returns the copy and the name it actually got' `
    ($null -ne $copy.menuId -and $copy.name -like '* copy*') `
    ("'$($copy.name)' ($($copy.menuId))")

$after = Expand-Api (Invoke-Api GET '/menus')
$copyCard = $after | Where-Object { $_.menuId -eq $copy.menuId }
Assert-That 'The copy is on the shelf, never published, on no screen' `
    ($null -eq $copyCard.board -and $null -eq $copyCard.publishedVersion -and @($copyCard.screenIds).Count -eq 0) `
    ("board null: $($null -eq $copyCard.board); version: '$($copyCard.publishedVersion)'; screens: $(@($copyCard.screenIds).Count)")

Assert-That 'The shelf grew by exactly one' (($after.Count - $before.Count) -eq 1) `
    ("$($before.Count) -> $($after.Count)")

# The same library item on both boards: sharing is the point of the library.
$sourceItems = @($card.board.sections | ForEach-Object { $_.items } | ForEach-Object { $_.itemId })
$copyDraft = Invoke-Api GET "/menus/$($copy.menuId)/draft"
$copyPlaced = @($copyDraft.changes | Where-Object { $_.targetKind -eq 'placement' } | ForEach-Object { $_.targetId })
$shared = @($sourceItems | Where-Object { $copyPlaced -contains $_ })
Assert-That 'The copy places the same library items as the original' `
    ($sourceItems.Count -eq 0 -or $shared.Count -gt 0) `
    ("$($shared.Count) of $($sourceItems.Count) source item(s) placed on the copy")

$copyHistory = Expand-Api (Invoke-Api GET "/menus/$($copy.menuId)/history")
$duplicated = @($copyHistory | Where-Object { $_.kind -eq 'duplicated' })
Assert-That 'The copy records where it came from, with its author' `
    ($duplicated.Count -eq 1 -and -not [string]::IsNullOrWhiteSpace($duplicated[0].author)) `
    ("$($duplicated.Count) entry: '$($duplicated[0].detail)' by '$($duplicated[0].author)'")

$second = Invoke-Api POST "/menus/$menuId/duplicate"
Assert-That 'A second copy gets its own name rather than colliding' `
    ($second.name -ne $copy.name) `
    ("'$($copy.name)' then '$($second.name)'")

# ---- clean up ------------------------------------------------------------------
# The copies this run made are removed here rather than through the API: there is
# deliberately no delete-by-pattern endpoint on the deployed system, the same reason
# start-ui-test-env.ps1 prunes its seed in SQL.
$copyIds = @($copy.menuId, $second.menuId) | Where-Object { $_ }
if ($copyIds.Count -gt 0) {
    $idList = ($copyIds | ForEach-Object { "'$_'" }) -join ','
    $cleanup = @"
SET NOCOUNT ON;
DECLARE @Copies TABLE (Id uniqueidentifier);
INSERT INTO @Copies (Id) SELECT Id FROM dbo.Menus WHERE Id IN ($idList);
DELETE h FROM dbo.MenuHistoryEntries h INNER JOIN @Copies c ON c.Id = h.MenuId;
DELETE p FROM dbo.Placements p INNER JOIN @Copies c ON c.Id = p.MenuId;
DELETE s FROM dbo.MenuSections s INNER JOIN @Copies c ON c.Id = s.MenuId;
DELETE m FROM dbo.Menus m INNER JOIN @Copies c ON c.Id = m.Id;
"@
    & sqlcmd -S '(localdb)\MSSQLLocalDB' -d VennuSign -E -b -I -Q $cleanup | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Host "Cleanup failed; $($copyIds.Count) duplicate menu(s) remain." -ForegroundColor Yellow }
    else { Write-Host "`nRemoved the $($copyIds.Count) menu(s) this run created." }
}

Write-Host ("`n{0} of {1} checks passed`n" -f ($checks - $fails), $checks) -ForegroundColor $(if ($fails -eq 0) { 'Green' } else { 'Red' })
if ($fails -gt 0) { exit 1 }
