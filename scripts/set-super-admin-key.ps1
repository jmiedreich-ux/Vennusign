[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$bytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$key = $null

try {
    $rng.GetBytes($bytes)
    $key = [Convert]::ToBase64String($bytes)
    [Environment]::SetEnvironmentVariable('SuperAdmin__ApiKey', $key, [EnvironmentVariableTarget]::User)

    $setClipboard = Get-Command Set-Clipboard -ErrorAction SilentlyContinue
    if ($null -eq $setClipboard) {
        throw 'Set-Clipboard is not available in this PowerShell session.'
    }

    Set-Clipboard -Value $key
    Write-Host 'A new temporary Super Admin key was saved to the current Windows user environment and copied to the clipboard.'
    Write-Host 'Close and reopen Vennu Development Control, restart API, then paste the clipboard value into Super Admin access.'
}
finally {
    $rng.Dispose()
    [Array]::Clear($bytes, 0, $bytes.Length)
    $key = $null
}
