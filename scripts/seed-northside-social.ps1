param(
    [string]$TestApiBaseUrl = "https://localhost:7140",
    [string]$FrontendBaseUrl = "https://localhost:5177",
    [string]$AccessToken = "track1-owner-review",
    [string]$TestApiKey
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($TestApiKey)) {
    $keyPath = Join-Path $PSScriptRoot "..\artifacts\ui-test-env\test-api.key"
    if (-not (Test-Path $keyPath)) {
        throw "The Test API key was not found. Pass -TestApiKey or start the local Test API environment first."
    }

    $TestApiKey = (Get-Content $keyPath -Raw).Trim()
}

$payload = @{
    accessToken = $AccessToken
    includeScreen = $false
    showcase = "northside-social"
    label = "northside-social"
    pageCount = 3
} | ConvertTo-Json

$tempPayload = [IO.Path]::GetTempFileName()
try {
    [IO.File]::WriteAllText($tempPayload, $payload, [Text.UTF8Encoding]::new($false))
    $response = & curl.exe -k -sS -f `
        -H "X-Vennusign-Test-Api-Key: $TestApiKey" `
        -H "Content-Type: application/json" `
        --data-binary "@$tempPayload" `
        "$TestApiBaseUrl/api/test/seed"

    if ($LASTEXITCODE -ne 0) {
        throw "The Northside Social seed request failed."
    }
}
finally {
    [IO.File]::Delete($tempPayload)
}

$seed = $response | ConvertFrom-Json
$menuUrl = "$FrontendBaseUrl/#/menu/$($seed.menuId)"
Write-Host ""
Write-Host "Northside Social is ready:" -ForegroundColor Green
Write-Host $menuUrl -ForegroundColor Cyan
