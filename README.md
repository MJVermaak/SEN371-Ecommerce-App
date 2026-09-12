# GrandmastersHub

A full-stack e-commerce application for SEN371, built with ASP.NET Core, Entity Framework Core, SQL Server, React, and Vite.

## Screenshots

### Storefront

![GrandmastersHub storefront](docs/screenshots/home.png)

### Product catalog

![GrandmastersHub chess board catalog](docs/screenshots/catalog.png)

## Prerequisites

- .NET 10 SDK
- Node.js and npm
- SQL Server (Developer or Express)
- Git

The checked-in connection string uses Windows authentication and expects SQL Server on `localhost`. See [Database setup](docs/database-setup.md) if your SQL Server configuration differs.

## Run locally

Start SQL Server, then open two PowerShell terminals.

### Terminal 1: API

```powershell
Set-Location 'D:\dev\SEN371-Ecommerce-App'

# Generate a signing key for this local session.
$keyBytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $rng.GetBytes($keyBytes)
}
finally {
    $rng.Dispose()
}

# Override missing or empty configuration.
$env:Jwt__SigningKey = [Convert]::ToBase64String($keyBytes)
$env:DOTNET_ENVIRONMENT = 'Development'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Database__ApplyMigrationsOnStartup = 'true'

dotnet run --project .\GrandmastersHub\GrandmastersHub.Api --no-launch-profile -- --urls http://localhost:5188
```

On startup, the API applies the committed database migrations. Leave this terminal running.

### Terminal 2: frontend

```powershell
Set-Location 'D:\dev\SEN371-Ecommerce-App\GrandmastersHub\GrandmastersHub.Api\client'
npm install
npm run dev
```

Open the Vite URL shown in the terminal. API requests from the frontend are proxied to `http://localhost:5188`.

## Useful URLs

- API: `http://localhost:5188`
- Swagger: `http://localhost:5188/swagger`

## Project structure

- `GrandmastersHub.Domain` — entities and repository contracts
- `GrandmastersHub.Application` — use cases, services, validation, and DTOs
- `GrandmastersHub.Infrastructure` — EF Core, SQL Server, and repository implementations
- `GrandmastersHub.Api` — REST API and React client
- `GrandmastersHub.Tests` — automated tests

## Tests

From the repository root:

```powershell
dotnet test .\GrandmastersHub\GrandmastersHub.slnx

Set-Location .\GrandmastersHub\GrandmastersHub.Api\client
npm test
npm run lint
```

The generated JWT signing key exists only in the API terminal's process environment and is not saved to source control. Use a managed secret with a random key of at least 32 bytes for deployment.
