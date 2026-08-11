[CmdletBinding()]
param(
    [switch]$SkipDisplay,
    [switch]$SkipBackOffice,
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

    <#
        The front ends.

        Both are here deliberately. Only the display app used to be, and the back
        office's production build was therefore never run by any gate - so milestone
        3 reached an independent review with a branch whose back office did not
        compile, while `npm test` and the whole Playwright suite were green. The dev
        server transforms per module and never type-checks the project, so nothing
        anybody ran locally could have seen it. An app that ships is an app that is
        built here.
    #>
    $apps = @(
        @{ Name = 'Display'; Path = Join-Path $root 'src/display'; Skip = $SkipDisplay },
        @{ Name = 'Back Office'; Path = Join-Path $root 'src/back-office'; Skip = $SkipBackOffice }
    )

    foreach ($app in $apps) {
        $packageJson = Join-Path $app.Path 'package.json'
        if ($app.Skip -or -not (Test-Path $packageJson)) {
            if ($app.Skip) { Write-Host "Skipping $($app.Name)." }
            continue
        }

        Write-Host "Validating $($app.Name)..."
        Push-Location $app.Path
        try {
            if (Test-Path (Join-Path $app.Path 'package-lock.json')) {
                npm ci
            }
            else {
                npm install
            }
            if ($LASTEXITCODE -ne 0) { throw "$($app.Name) dependency installation failed." }

            $package = Get-Content $packageJson -Raw | ConvertFrom-Json
            if ($package.scripts.test) {
                # The display app's runner needs --run; the back office uses node --test.
                if ($app.Name -eq 'Display') { npm test -- --run } else { npm test }
                if ($LASTEXITCODE -ne 0) { throw "$($app.Name) tests failed." }
            }
            if ($package.scripts.build) {
                npm run build
                if ($LASTEXITCODE -ne 0) { throw "$($app.Name) build failed." }
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
