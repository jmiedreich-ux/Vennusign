Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$repoRoot = Split-Path -Parent $PSScriptRoot
$services = @(
    [pscustomobject]@{ Name = 'API'; Port = 5192; Directory = $repoRoot; Command = 'dotnet run --no-build --launch-profile http --project .\src\Vennu.Api\Vennu.Api.csproj'; Environment = @{ ASPNETCORE_ENVIRONMENT = 'Development' }; Url = 'http://localhost:5192' },
    [pscustomobject]@{ Name = 'Admin'; Port = 5173; Directory = "$repoRoot\src\admin"; Command = 'npm run dev -- --host localhost --port 5173'; Environment = @{ VITE_VENNU_API_BASE_URL = 'http://localhost:5192'; VITE_VENNU_DISPLAY_BASE_URL = 'http://localhost:5175'; VITE_VENNU_VENUE_ADMIN_BASE_URL = 'http://localhost:5174/venue-admin/' }; Url = 'http://localhost:5173' },
    [pscustomobject]@{ Name = 'Venue Admin'; Port = 5174; Directory = "$repoRoot\src\venue-admin"; Command = 'npm run dev -- --host localhost --port 5174'; Environment = @{ VITE_VENNU_API_BASE_URL = 'http://localhost:5192' }; Url = 'http://localhost:5174/venue-admin/' },
    [pscustomobject]@{ Name = 'Display'; Port = 5175; Directory = "$repoRoot\src\display"; Command = 'npm run dev -- --host localhost --port 5175'; Environment = @{ VITE_API_BASE_URL = 'http://localhost:5192'; VITE_SIGNALR_HUB_URL = 'http://localhost:5192/hubs/vennu' }; Url = 'http://localhost:5175' }
)
$owned = @{}

function Test-ServicePort([int]$Port) { [bool](Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue) }
function Start-Service($Service) {
    if (Test-ServicePort $Service.Port) { return }
    $environment = ($Service.Environment.GetEnumerator() | ForEach-Object { "`$env:$($_.Key) = '$($_.Value)'" }) -join '; '
    $script = "Set-Location '$($Service.Directory)'; $environment; $($Service.Command)"
    $owned[$Service.Name] = Start-Process powershell -ArgumentList '-NoExit', '-Command', $script -PassThru
}
function Stop-Service($Service) {
    if ($owned.ContainsKey($Service.Name) -and -not $owned[$Service.Name].HasExited) { Stop-Process -Id $owned[$Service.Name].Id -Force }
    $owned.Remove($Service.Name)
}

$form = New-Object System.Windows.Forms.Form
$form.Text = 'Vennu Development Control'
$form.Size = New-Object System.Drawing.Size(680, 330)
$form.StartPosition = 'CenterScreen'
$layout = New-Object System.Windows.Forms.TableLayoutPanel
$layout.Dock = 'Fill'; $layout.ColumnCount = 6; $layout.RowCount = $services.Count + 2
$layout.ColumnStyles.Add((New-Object System.Windows.Forms.ColumnStyle([System.Windows.Forms.SizeType]::Percent, 25)))
1..5 | ForEach-Object { $layout.ColumnStyles.Add((New-Object System.Windows.Forms.ColumnStyle([System.Windows.Forms.SizeType]::Percent, 15))) }
$null = $form.Controls.Add($layout)

$labels = @{}
$row = 0
foreach ($service in $services) {
    $name = New-Object System.Windows.Forms.Label; $name.Text = "$($service.Name) ($($service.Port))"; $name.AutoSize = $true
    $status = New-Object System.Windows.Forms.Label; $status.AutoSize = $true; $labels[$service.Name] = $status
    $start = New-Object System.Windows.Forms.Button; $start.Text = 'Start'; $start.Add_Click({ Start-Service $this.Tag; Update-Status }) ; $start.Tag = $service
    $stop = New-Object System.Windows.Forms.Button; $stop.Text = 'Stop'; $stop.Add_Click({ Stop-Service $this.Tag; Update-Status }); $stop.Tag = $service
    $restart = New-Object System.Windows.Forms.Button; $restart.Text = 'Restart'; $restart.Add_Click({ Stop-Service $this.Tag; Start-Sleep -Milliseconds 500; Start-Service $this.Tag; Update-Status }); $restart.Tag = $service
    $open = New-Object System.Windows.Forms.Button; $open.Text = 'Open'; $open.Add_Click({ Start-Process $this.Tag.Url }); $open.Tag = $service
    $layout.Controls.Add($name, 0, $row); $layout.Controls.Add($status, 1, $row); $layout.Controls.Add($start, 2, $row); $layout.Controls.Add($stop, 3, $row); $layout.Controls.Add($restart, 4, $row); $layout.Controls.Add($open, 5, $row)
    $row++
}
$all = New-Object System.Windows.Forms.Button; $all.Text = 'Start All'; $all.Add_Click({ $services | ForEach-Object { Start-Service $_ }; Update-Status })
$stopAll = New-Object System.Windows.Forms.Button; $stopAll.Text = 'Stop Owned'; $stopAll.Add_Click({ $services | ForEach-Object { Stop-Service $_ }; Update-Status })
$layout.Controls.Add($all, 2, $row); $layout.Controls.Add($stopAll, 3, $row)
function Update-Status { $services | ForEach-Object { $labels[$_.Name].Text = if (Test-ServicePort $_.Port) { 'Running' } else { 'Stopped' } } }
$timer = New-Object System.Windows.Forms.Timer; $timer.Interval = 1500; $timer.Add_Tick({ Update-Status }); $timer.Start(); Update-Status
[void]$form.ShowDialog()
