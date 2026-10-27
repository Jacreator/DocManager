# DocManager

A small document management system built with ASP.NET Core MVC (.NET 10), EF Core 10 and SQL Server / Azure SQL.

Users can upload documents, browse and search them, pin notes onto a document preview (AJAX, no page reload),
soft delete documents and restore them from a recycle bin. Every query is scoped to the current company.

## Architecture

Dependencies point inward only:

- **DocManager.Domain** is the core and depends on no other project.
- **DocManager.Application** depends on Domain.
- **DocManager.Infrastructure** depends on Application and Domain, and implements their interfaces (repositories, unit of work, file storage).
- **DocManager.Web** depends on Application for services and DTOs. It references Infrastructure only to call `AddInfrastructure` during DI registration.
- **DocManager.Tests** depends on Web, and through it on all other projects.

| Project | Responsibility |
| --- | --- |
| `DocManager.Domain` | `BaseEntity`, `Document`, `DocumentNote`, `PagedResult<T>`, repository and unit of work interfaces. No package dependencies. |
| `DocManager.Application` | `DocumentService`, `NoteService`, DTOs, FluentValidation validators, `Result`/`Result<T>`, `DocumentOptions`, and the `IFileStorage` / `ICurrentCompany` abstractions. |
| `DocManager.Infrastructure` | `AppDbContext` (global query filters, UTC `DateAdded` stamping), `IEntityTypeConfiguration` mappings, generic `Repository<T>`, `UnitOfWork`, `LocalFileStorage`, migrations and seed data. |
| `DocManager.Web` | Thin controllers, view models, Razor views and partials, `fetchJson` helper, global exception handler, `HttpCurrentCompany`. |
| `DocManager.Tests` | xUnit: service unit tests (Moq), repository tests (SQLite in-memory), integration tests (`WebApplicationFactory`). |

### Request flow example: adding a note

1. `document-notes.js` turns the click position into percentages of the viewer and calls `fetchJson('/Documents/1/Notes', { method: 'POST', body })`.
2. `fetchJson` adds the anti-forgery token from `<meta name="csrf-token">` as the `X-CSRF-TOKEN` header.
3. `NotesController.Create` (`[ValidateAntiForgeryToken]`) calls `INoteService.AddNoteAsync`.
4. `NoteService` validates (`CreateNoteRequestValidator`), checks the document exists (the query filter hides deleted documents and other companies' documents), adds the note via `IDocumentNoteRepository` and commits with `IUnitOfWork`.
5. The controller maps the `Result` to `201 Created` + `NoteDto`, `400` or `404` (ProblemDetails), and the JS renders the marker.

## Getting started

### Prerequisites

- .NET 10 SDK. All projects target `net10.0` (set once in `Directory.Build.props`).
- SQL Server: Docker (recommended on macOS/Linux), LocalDB (Windows) or Azure SQL.
- EF Core CLI: `dotnet tool install --global dotnet-ef` (or `dotnet tool update --global dotnet-ef`)

### 1. Configure the connection string

`appsettings.Development.json` points at LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=DocManager;Trusted_Connection=True;...
```

On macOS/Linux, run SQL Server in Docker and store the connection string in **User Secrets** (never commit passwords).
User Secrets override `appsettings.Development.json`:

```bash
docker run --name SQL_Server -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_strong_Passw0rd" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2025-latest

cd src/DocManager.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=DocManager;User Id=sa;Password=Your_strong_Passw0rd;TrustServerCertificate=True"
```

### 2. Create the database

In Development, pending migrations are applied automatically on startup (`Database:MigrateOnStartup`).
To apply them manually:

```bash
dotnet ef database update -p src/DocManager.Infrastructure/DocManager.Infrastructure.csproj -s src/DocManager.Web/DocManager.Web.csproj
```

The migrations seed sample data for two companies. The files ship in
`src/DocManager.Web/App_Data/uploads`, so previews work straight away.

| Company | Documents | Recycle bin | Notes |
| --- | --- | --- | --- |
| 1 | Employee Handbook, Office Floor Plan, Quarterly Report Q1 (Ids 1–3) | – | – |
| 2 | Supplier Contract, Warehouse Layout, Board Meeting Minutes (Ids 1001–1003) | Price List 2025 (Id 1004) | One note on Supplier Contract |

*The seed data is sample data for testing in a local development environment only, not real data.
Seed rows are inserted by their migration, and each migration runs only once per database: EF Core records it in
`__EFMigrationsHistory` and skips it on later runs. So the seed is never inserted again once it is in the database,
and a seed row you delete stays deleted. Seed rows added after the initial migration use Ids from 1001 upwards,
because real uploads get identity Ids and seed rows with low Ids could collide with them in an existing database.*

### 3. Run

```bash
dotnet build
dotnet run --project src/DocManager.Web
```

Then browse to <http://localhost:5285>. This runs as company 1; see
[Running as a different company](#running-as-a-different-company) to switch.

### Running as a different company

There is no login yet, so the app decides which company you are from the `Company:DefaultCompanyId` setting.
It is `1` in `appsettings.json`, so `dotnet run` always runs as company 1.

To run as **company 2**, start the app like this:

```bash
Company__DefaultCompanyId=2 dotnet run --project src/DocManager.Web
```

How the command works:

- `Company__DefaultCompanyId=2` sets an environment variable for this one command only.
  ASP.NET Core reads environment variables as configuration, and the double underscore `__` stands for the `:`
  section separator. So this overrides `Company:DefaultCompanyId` from `appsettings.json` without editing any file.
- Nothing is saved. Run plain `dotnet run --project src/DocManager.Web` again to go back to company 1.
- Only one copy of the app can listen on port 5285. Stop the running copy (`Ctrl+C`) before starting it as another company.

On Windows, set the variable first:

```powershell
# PowerShell
$env:Company__DefaultCompanyId = "2"; dotnet run --project src/DocManager.Web
```

```cmd
:: Command Prompt
set Company__DefaultCompanyId=2 && dotnet run --project src/DocManager.Web
```

In PowerShell and Command Prompt the variable stays set for the rest of that terminal session. Close the terminal,
or set it back to `1`, to return to company 1.

#### Checking that companies can't see each other's data

1. Run as company 1 and open <http://localhost:5285>. You see Employee Handbook, Office Floor Plan and Quarterly Report Q1.
2. Note the URL of one of your documents, for example <http://localhost:5285/Documents/Details/1>.
3. Stop the app and start it as company 2. You now see Supplier Contract, Warehouse Layout and Board Meeting Minutes,
   and Price List 2025 is in the recycle bin.
4. Open the company 1 URL from step 2. You get a "not found" page, and `/Documents/File/1` returns 404 too.
   Company 1 gets the same result for company 2's documents (for example `/Documents/Details/1001`).

The app returns "not found" rather than "access denied", so it doesn't reveal that another company's document exists.
`CompanyIsolationTests` checks all of this automatically for listing, search, details, files, notes, delete,
the recycle bin and restore.

> This setting is a stand-in until real authentication exists. Then the company will come from the signed-in user's
> `company_id` claim (see `HttpCurrentCompany`), and users won't be able to choose it.

### Working with migrations

```bash
# Add a migration after changing entities/configurations
dotnet ef migrations add <Name> -p src/DocManager.Infrastructure/DocManager.Infrastructure.csproj -s src/DocManager.Web/DocManager.Web.csproj -o Persistence/Migrations

# Generate an idempotent SQL script (for CI/CD or a DBA)
dotnet ef migrations script --idempotent -p src/DocManager.Infrastructure/DocManager.Infrastructure.csproj -s src/DocManager.Web/DocManager.Web.csproj -o migrate.sql
```

### Running the tests

```bash
dotnet test
```

No SQL Server is needed: repository and integration tests use SQLite in-memory databases.

## Configuration

| Key | Default | Purpose |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | – | SQL Server / Azure SQL connection string. |
| `Documents:MaxFileSizeBytes` | `10485760` | Maximum upload size (10 MB). |
| `Documents:AllowedExtensions` | `.pdf .docx .png .jpg` | Allowed upload extensions. |
| `Documents:PageSize` | `10` | Page size for lists. |
| `FileStorage:RootPath` | `App_Data/uploads` | Upload folder (absolute, or relative to the content root). Outside `wwwroot`. |
| `Company:DefaultCompanyId` | `1` | Company used when the user has no `company_id` claim (until real auth exists). |
| `Database:MigrateOnStartup` | `true` in Development | Apply migrations at startup. Keep it off in production. |

### Secrets in Azure

Don't store connection strings in `appsettings.json` for deployed environments:

- **App Service configuration**: add a *Connection string* named `DefaultConnection` (type SQLAzure), or an
  app setting `ConnectionStrings__DefaultConnection`. App Service exposes it to ASP.NET Core configuration automatically.
- **Azure Key Vault**: store the secret in Key Vault and either
  - reference it from an App Service setting: `@Microsoft.KeyVault(SecretUri=https://<vault>.vault.azure.net/secrets/DefaultConnection/)`, or
  - load the vault into configuration with `builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential())`
    (packages `Azure.Extensions.AspNetCore.Configuration.Secrets` and `Azure.Identity`), naming the secret `ConnectionStrings--DefaultConnection`.
- Prefer a **managed identity** with Azure SQL (`Authentication=Active Directory Default`) so there is no password at all.

## Design decisions

### Why Repository + Unit of Work on top of EF Core?

`DbContext` already is a unit of work and `DbSet` already is a repository, so this is a deliberate choice:

- **Application stays persistence-agnostic.** Services depend on `IDocumentRepository`/`IUnitOfWork` (Domain), not on EF Core. They can be unit tested with Moq and no database.
- **Queries live in one place.** "Search by title, newest first, paged" is defined once in `DocumentRepository` instead of being rebuilt in each service. Repositories return materialised results, never `IQueryable`, so no query building leaks into upper layers.
- **Explicit transaction boundary.** Repositories only stage changes. A service decides when the business operation is complete and calls `IUnitOfWork.SaveChangesAsync` once.
- **Kept thin.** A generic `Repository<T>` holds the shared `GetByIdAsync`/`AddAsync`/`Update`/paging code. Repositories contain no business rules. Validation and decisions live in services.

### Other decisions

- **Multi-tenancy and soft delete via global query filters.** `Document` is filtered by `DeletedDate == null && CompanyId == current company`. `DocumentNote` has a matching filter through its `Document` navigation, so notes of hidden documents are hidden too. `IgnoreQueryFilters()` removes *all* filters, so the recycle bin queries re-apply the company condition explicitly.
- **Result pattern.** Services return `Result`/`Result<T>` (`Success`, `NotFound`, `ValidationError`) instead of throwing for expected outcomes. Controllers only translate results into HTTP responses or views. Exceptions are reserved for unexpected failures.
- **Validation in one place.** FluentValidation validators in the Application layer are the single source of rules. The MVC form model has no duplicate attributes; service errors are copied into `ModelState`.
- **File security.** Extension and size are validated against `DocumentOptions`. Files are stored outside `wwwroot` under a GUID name, so user-controlled names never reach the file system. `LocalFileStorage` additionally rejects anything that isn't a plain file name. The content type comes from the extension on the server, not the client header. Files are served only through `DocumentsController.File`, which goes through the query filters (company + not deleted), with `X-Content-Type-Options: nosniff`. If the database save fails after the file was written, the file is removed.
- **Pluggable storage.** `IFileStorage` (`SaveAsync`, `OpenReadAsync`, `DeleteAsync`) is all the services know. To move to Azure Blob Storage, add an `AzureBlobFileStorage : IFileStorage` in Infrastructure and change one DI registration. No service changes are needed.
- **Anti-forgery everywhere.** `AutoValidateAntiforgeryToken` is a global filter. Forms use the hidden token, and AJAX sends it in the `X-CSRF-TOKEN` header via the shared `fetchJson` helper.
- **Errors.** `GlobalExceptionHandler` (an `IExceptionHandler` used by the exception handler middleware) returns ProblemDetails for JSON requests. MVC requests fall through to the friendly `/Error` page. Status codes such as 404 render friendly pages via status code page re-execution.
- **UTC everywhere.** `DateAdded` is stamped in `AppDbContext.SaveChangesAsync`, and `SoftDelete()` uses `DateTime.UtcNow`. The UI labels times as UTC.
- **Note positions** are percentages (0–100, `decimal(5,2)`) of the viewer box, so markers stay in place when the viewer is resized.

## Assumptions and notes

- **Authentication is out of scope.** `HttpCurrentCompany` reads a `company_id` claim and falls back to `Company:DefaultCompanyId` (marked with a TODO). Add real authentication before going to production.
- **View models live in the Web project** (`Models/`). They are presentation concerns, and the upload form needs `IFormFile`, which the Application layer must not depend on. View models may wrap Application DTOs, but entities never reach views.
- **Previews:** PDFs are shown in an `<iframe>` and images inline. `.docx` has no in-browser preview, so a placeholder with a download link is shown, which can still be annotated. Toggle **Add notes** to place notes; while it is off the PDF can be scrolled normally.
- **Package pin:** `System.Security.Cryptography.Pkcs` is referenced explicitly in Infrastructure to override the vulnerable 9.0.11 version that `Microsoft.Data.SqlClient` 6.1.x pulls in. Remove the pin once SqlClient ships a fixed dependency.
