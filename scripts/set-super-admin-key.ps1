[CmdletBinding()]
param(
    [switch]$ReuseExisting
)

$ErrorActionPreference = 'Stop'
$bytes = $null
$rng = $null
$key = if ($ReuseExisting) { [Environment]::GetEnvironmentVariable('SuperAdmin__ApiKey', [EnvironmentVariableTarget]::User) } else { $null }
$generated = [string]::IsNullOrWhiteSpace($key)

try {
    if ($generated) {
        $bytes = New-Object byte[] 32
        $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        $rng.GetBytes($bytes)
        $key = [Convert]::ToBase64String($bytes)
        [Environment]::SetEnvironmentVariable('SuperAdmin__ApiKey', $key, [EnvironmentVariableTarget]::User)
    }

    $setClipboard = Get-Command Set-Clipboard -ErrorAction SilentlyContinue
    if ($null -eq $setClipboard) {
        throw 'Set-Clipboard is not available in this PowerShell session.'
    }

    Set-Clipboard -Value $key
    if ($generated) {
        Write-Host 'A new temporary Super Admin key was saved to the current Windows user environment and copied to the clipboard.'
        Write-Host 'Close and reopen Vennu Development Control, restart API, then paste the clipboard value into Super Admin access.'
    }
    else {
        Write-Host 'The existing Super Admin access key was copied to the clipboard.'
    }
}
finally {
    if ($null -ne $rng) { $rng.Dispose() }
    if ($null -ne $bytes) { [Array]::Clear($bytes, 0, $bytes.Length) }
    $key = $null
}
