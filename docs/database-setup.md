# Database setup and diagnostics

## What failed

The supplied log reports SQL Server error 4060 for `GrandmastersHubDb` on
`localhost`. SQL Server was reached, but the requested database could not be
opened by the login. Check whether the database exists on that instance, whether
it is online, and whether the login has access. The log alone does not distinguish
those cases.

At the reviewed baseline, `d212b00c208b6c32eeb02fc02f25febc2668d23c`, startup
migrations were commented out and the infrastructure project contained no
migrations. Enabling `MigrateAsync` alone would not supply the missing schema.
The multiple-collection query warning is separate from the failed login.

## First setup

Start your own local SQL Server instance. From the repository root, run:

```powershell
.\scripts\Initialize-Database.ps1
dotnet run --project .\GrandmastersHub\GrandmastersHub.Api
```

The script restores `dotnet-ef` 10.0.11 to match the existing EF package version.
It uses the API's Development configuration, including user secrets and any
`ConnectionStrings__DefaultConnection` environment override. It does not print
connection strings or write credentials into the repository.

If there are no migrations in the infrastructure `Migrations` directory, the
script generates `InitialCreate` and the model snapshot with EF's own tooling.
It checks for pending model changes and runs `dotnet ef database update`. Repeated
runs use the existing migration files. The script restores its temporary process
environment settings on success or failure.

The patch does not contain a generated initial migration. Generate it once,
review the files, and commit the entire `Migrations` directory before teammates
run setup. That gives everyone the same migration IDs. The setup script needs
the .NET 10 SDK, NuGet access, and SQL Server access.

For SQL Express, change the connection only when this matches your installed
instance. Keep the override in the same PowerShell session for setup and the API:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost\SQLEXPRESS;Database=GrandmastersHubDb;Trusted_Connection=True;TrustServerCertificate=True;'
.\scripts\Initialize-Database.ps1
dotnet run --project .\GrandmastersHub\GrandmastersHub.Api
```

For a persistent local setting, initialize API user secrets and store
`ConnectionStrings:DefaultConnection` there. Follow the root README's existing
user-secrets instructions. Do not commit a real password in `appsettings.json`.
Non-Windows development needs a suitable SQL Server authentication configuration;
the checked-in Windows integrated-authentication connection is not a portable
login configuration.

## Existing databases and permissions

Do not delete a database or grant broad server roles to resolve this error.
If a database already has application tables without an EF migration history,
stop and review its schema and backup before applying the initial migration.
The initial migration is for an empty database. This script does not adopt or
baseline an existing schema, and it does not call `EnsureCreated` or `EnsureDeleted`.

The setup login needs access to the target database and permissions to apply its
schema changes. Creating a missing database also needs database-creation rights.
A database administrator can instead create an empty database and give the setup
identity the required database permissions. A deployed runtime account should
not need permission to create databases or apply migrations.

When a login can inspect `master`, these read-only checks can help distinguish a
missing database from an inaccessible one. Results can be hidden by permissions:

```sql
SELECT name, state_desc
FROM sys.databases
WHERE name = N'GrandmastersHubDb';

SELECT HAS_DBACCESS(N'GrandmastersHubDb') AS HasDatabaseAccess;
```

`EnableRetryOnFailure` is not a replacement for a schema or access permissions.
The patch leaves the existing retry policy unchanged.

## Startup behavior

Development startup applies migrations before accepting requests. It stops with
an actionable message when no migrations exist or SQL initialization fails.
Other environments do not run startup migrations, even when the setting is true.
Use reviewed migration scripts or bundles as a separate deployment step.

For routing-only development without a database:

```powershell
$env:Database__ApplyMigrationsOnStartup = 'false'
dotnet run --project .\GrandmastersHub\GrandmastersHub.Api
```

This only bypasses initialization. Product, category, authentication, and other
database-backed requests still need a working database. Remove the override to
restore initialization:

```powershell
Remove-Item Env:Database__ApplyMigrationsOnStartup
```

Product list, ID, and slug reads use `AsSplitQuery` for their included collections.
That addresses the multiple-collection warning without suppressing EF warnings or
changing query behavior globally. Split queries add round trips and do not imply
a transactionally consistent snapshot across concurrently changing collections.

## Verification

Run the new startup-policy tests without a SQL Server connection:

```powershell
dotnet test .\GrandmastersHub\GrandmastersHub.Tests --filter FullyQualifiedName~DatabaseStartupTests
```

The tests cover the default Development behavior, explicit enable/disable, and
non-Development environments. They replace the context with a throwing factory
so an unexpected database resolution fails the test.

After database setup, start the API and check the affected reads:

```powershell
Invoke-RestMethod http://localhost:5188/api/v1/products
Invoke-RestMethod http://localhost:5188/api/v1/categories
```

An empty database can return empty catalog arrays. Check a product ID returned by
the product list to exercise the detail query. Confirm that logs contain neither
error 4060 nor `MultipleCollectionIncludeWarning` for those product reads.

The existing persistence tests use their own hard-coded SQL connection to
`GrandmastersHubDb` and some tests write data. Do not run the whole suite against a
valuable database. These test-connection defaults are unchanged by this patch.

## Validation limits of this patch

The patch was prepared through read access to the repository. GitHub rejected
both Git tree creation and a source-file update with HTTP 403, so no remote file,
branch, workflow, or commit was changed. The local environment had no .NET SDK,
PowerShell, or SQL Server. The GitHub runner could not be used because the workflow
write was denied. The C# tests, PowerShell script, migration generation, and live
SQL queries were not executed.

The modified-file baselines were verified against their fetched Git blob hashes.
Patch application, whitespace, manifest JSON, and targeted static checks were
validated locally. These are not a substitute for compilation or runtime tests.

## References

- Microsoft EF schema initialization guidance: https://learn.microsoft.com/en-us/ef/core/managing-schemas/ensure-created
- Microsoft EF split-query guidance: https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries
- Microsoft EF CLI reference: https://learn.microsoft.com/en-us/ef/core/cli/dotnet
