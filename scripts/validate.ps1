[CmdletBinding()]
param(
    [switch]$SkipDisplay,
    [switch]$SkipIntegration
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root

try {
    $solution = Get-ChildItem -Path $root -Filter *.sln -File | Select-Object -First 1
    if (-not $solution) {
        throw 'No solution file was found at the repository root.'
    }

    Write-Host "Restoring $($solution.Name)..."
    dotnet restore $solution.FullName
    if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

    Write-Host "Building $($solution.Name)..."
    dotnet build $solution.FullName --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

    $testProjects = Get-ChildItem -Path (Join-Path $root 'tests') -Filter *.csproj -File -Recurse -ErrorAction SilentlyContinue
    foreach ($project in $testProjects) {
        $isIntegration = $project.Name -match 'Integration|E2E'
        if ($SkipIntegration -and $isIntegration) {
            Write-Host "Skipping integration project $($project.Name)."
            continue
        }

        Write-Host "Testing $($project.Name)..."
        dotnet test $project.FullName --no-build
        if ($LASTEXITCODE -ne 0) { throw "Tests failed for $($project.Name)." }
    }

    $displayPath = Join-Path $root 'src/display'
    $packageJson = Join-Path $displayPath 'package.json'
    if (-not $SkipDisplay -and (Test-Path $packageJson)) {
        Push-Location $displayPath
        try {
            if (Test-Path (Join-Path $displayPath 'package-lock.json')) {
                npm ci
            }
            else {
                npm install
            }
            if ($LASTEXITCODE -ne 0) { throw 'Display dependency installation failed.' }

            $package = Get-Content $packageJson -Raw | ConvertFrom-Json
            if ($package.scripts.test) {
                npm test -- --run
                if ($LASTEXITCODE -ne 0) { throw 'Display tests failed.' }
            }
            if ($package.scripts.build) {
                npm run build
                if ($LASTEXITCODE -ne 0) { throw 'Display build failed.' }
            }
        }
        finally {
            Pop-Location
        }
    }

    Write-Host 'Validation completed successfully.'
}
finally {
    Pop-Location
}
