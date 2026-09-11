#Requires -Version 5.1
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-DotNet {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed with exit code $LASTEXITCODE. Fix the reported error before continuing."
    }
}

$root = Split-Path -Parent $PSScriptRoot
$infrastructure = Join-Path $root 'GrandmastersHub/GrandmastersHub.Infrastructure'
$api = Join-Path $root 'GrandmastersHub/GrandmastersHub.Api'
$migrations = Join-Path $infrastructure 'Migrations'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Install the .NET 10 SDK before running database setup.'
}

# Use the same Development configuration as dotnet run. Preserve caller overrides,
# including ConnectionStrings__DefaultConnection, without printing credentials.
$previousEnvironment = @{}
foreach ($name in @('ASPNETCORE_ENVIRONMENT', 'DOTNET_ENVIRONMENT', 'Database__ApplyMigrationsOnStartup')) {
    $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
}

Push-Location $root
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:DOTNET_ENVIRONMENT = 'Development'
    $env:Database__ApplyMigrationsOnStartup = 'false'

    Invoke-DotNet -Arguments @('tool', 'restore')

    # The reviewed baseline has no migrations. Generate the initial migration
    # from the real EF model rather than maintaining a separate handwritten schema.
    $existingMigrations = @()
    if (Test-Path -LiteralPath $migrations -PathType Container) {
        $existingMigrations = @(Get-ChildItem -LiteralPath $migrations -Recurse -File -Filter '*.cs' |
            Where-Object { $_.Name -match '^\d{14}_.+\.cs$' -and $_.Name -notlike '*.Designer.cs' })
    }

    if ($existingMigrations.Count -eq 0) {
        Write-Host 'Generating InitialCreate from the current EF Core model.'
        Invoke-DotNet -Arguments @(
            'ef', 'migrations', 'add', 'InitialCreate',
            '--project', $infrastructure, '--startup-project', $api,
            '--output-dir', 'Migrations'
        )
        Write-Host 'Review and commit the generated Migrations directory before sharing this setup with the team.'
    }

    # Refuse to apply an out-of-date migration history after a model change.
    Invoke-DotNet -Arguments @(
        'ef', 'migrations', 'has-pending-model-changes',
        '--project', $infrastructure, '--startup-project', $api
    )

    Write-Host 'Applying migrations using the API connection configuration.'
    Invoke-DotNet -Arguments @(
        'ef', 'database', 'update',
        '--project', $infrastructure, '--startup-project', $api
    )

    Write-Host 'Database setup completed. Start the API with dotnet run --project GrandmastersHub/GrandmastersHub.Api.'
}
finally {
    foreach ($name in $previousEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $previousEnvironment[$name], 'Process')
    }
    Pop-Location
}
