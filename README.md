# LeaveFlow

LeaveFlow is a v1.0 release candidate workforce leave, approval, calendar, reporting, and guarded AI assistant platform built as a public portfolio-ready .NET project.

It demonstrates ASP.NET Core MVC/Web API, .NET 10, SQL Server, Dapper, stored-procedure-only data access, RBAC, object-level authorization, workforce calendar/timeline/reporting, secure Azure AI tool calling, Docker-based local SQL Server, and 240+ automated tests.

## Features

- Consultant leave request creation, list, and detail workflow.
- Manager and administrator approve/reject workflow.
- Conflict detection from approved leave day rows.
- Inclusive `ConsultantLeaveDays` generation on approval.
- Administrator consultant/manager management and manager assignment.
- Organization holidays and official holidays.
- Role-aware workforce timeline, organization calendar, dashboard, and reports.
- Optional AI assistant with deterministic LeaveFlow workforce query planning.
- White-label UI branding through configuration.
- Light/dark MVC application shell.

## Architecture

- `LeaveFlow.Domain`: framework-independent domain constants and rules.
- `LeaveFlow.Application`: use cases, validation, authorization-aware orchestration, reporting, timeline, calendar, and AI tool boundaries.
- `LeaveFlow.Infrastructure`: SQL Server, Dapper stored-procedure repositories, password hashing, audit persistence, development bootstrap, and Azure AI provider integration.
- `LeaveFlow.Web`: ASP.NET Core MVC browser UI.
- `LeaveFlow.Api`: API host foundation with protected sample endpoint and health checks.

Database access is Dapper + stored procedures only. Application code must not use Entity Framework, `DbContext`, raw SQL, `CommandType.Text`, or generic query execution helpers.

## Tech Stack

- .NET 10
- ASP.NET Core MVC and Web API
- SQL Server 2022
- Dapper
- xUnit
- Docker Compose
- Optional Azure AI provider integration

## Project Structure

```text
src/
  LeaveFlow.Domain/
  LeaveFlow.Application/
  LeaveFlow.Infrastructure/
  LeaveFlow.Web/
  LeaveFlow.Api/
tests/
  LeaveFlow.UnitTests/
  LeaveFlow.IntegrationTests/
  LeaveFlow.SecurityTests/
db/
  001_Tables/
  002_Indexes/
  003_StoredProcedures/
  004_Seed/
docs/
```

## Screenshots

Screenshots are intentionally not committed yet. Recommended release screenshots:

- Login and dashboard
- Consultant leave request
- Manager review with conflict preview
- Workforce timeline
- Organization calendar
- AI assistant disabled/available state

## Prerequisites

- .NET 10 SDK
- Docker Desktop or another Docker engine
- PowerShell
- `sqlcmd` on the host, or use the SQL Server container mode in the setup script

## Quick Start

```powershell
git clone <your-fork-or-repo-url>
cd LeaveFlow
Copy-Item .env.example .env
```

Edit `.env` and set strong local values:

```text
LEAVEFLOW_SQL_PASSWORD
LeaveFlow__Development__DemoPassword
```

Start SQL Server:

```powershell
docker compose up -d sqlserver
```

If your Docker installation uses the older standalone Compose binary:

```powershell
docker-compose up -d sqlserver
```

Load local environment values:

```powershell
Get-Content .env | ForEach-Object {
    if ($_ -match '^\s*#' -or $_ -notmatch '=') { return }
    $name, $value = $_ -split '=', 2
    [Environment]::SetEnvironmentVariable($name.Trim(), $value.Trim(), 'Process')
}

$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=LeaveFlow;User Id=sa;Password=$env:LEAVEFLOW_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True"
```

Initialize the local database:

```powershell
.\scripts\setup-local-db.ps1 -DockerContainer leaveflow-sql
```

Run the MVC web app:

```powershell
dotnet run --project src\LeaveFlow.Web\LeaveFlow.Web.csproj
```

Open the URL shown by `dotnet run`, commonly:

```text
http://localhost:5226
```

## Docker SQL Setup

`docker-compose.yml` starts SQL Server only. `LeaveFlow.Web` and `LeaveFlow.Api` currently run through `dotnet run`.

`scripts/setup-local-db.ps1` is the single local database initialization path. It applies scripts in this order:

```text
db/001_Tables
db/002_Indexes
db/003_StoredProcedures
db/004_Seed
```

## Development Demo Users

When `ASPNETCORE_ENVIRONMENT=Development`, `LeaveFlow:Development:BootstrapIdentity=true`, and a demo password is configured, the web app creates or updates:

```text
admin@leaveflow.local
manager@leaveflow.local
consultant1@leaveflow.local
consultant2@leaveflow.local
```

The demo password is supplied through environment variables or user secrets and is never committed.

## White-Label Configuration

Branding is controlled by `LeaveFlow:Branding` options and does not change routes, authorization, database schema, business rules, or AI tools.

```powershell
$env:LeaveFlow__Branding__OrganizationName="Example Organization"
$env:LeaveFlow__Branding__ProductName="PeopleFlow"
$env:LeaveFlow__Branding__ShortName="PF"
$env:LeaveFlow__Branding__LogoUrl="/images/example-logo.svg"
$env:LeaveFlow__Branding__PrimaryBrandColor="#0f766e"
$env:LeaveFlow__Branding__SupportEmail="support@example.test"
$env:LeaveFlow__Branding__FooterText="Example Organization workforce operations"
```

Defaults are generic: `ProductName=LeaveFlow`, `OrganizationName=Organization`. Invalid colors, unsafe logo URLs, and invalid support emails are ignored.

## AI Architecture

AI is disabled by default and LeaveFlow runs normally without Azure AI credentials.

```powershell
$env:AI__Enabled="true"
$env:AI__Provider="Azure"
$env:AI__Azure__Endpoint="https://<your-resource>.openai.azure.com"
$env:AI__Azure__Deployment="<your-deployment>"
$env:AI__Azure__ApiVersion="2024-02-15-preview"
$env:AI__Azure__ApiKey="<your-api-key>"
```

The assistant can answer supported LeaveFlow workforce questions only through allowlisted read-only application tools. It has no direct SQL, repository, stored procedure, or database access. Counts, names, dates, statuses, conflicts, and availability facts must come from tool results.

## Security Highlights

- Cookie authentication for the MVC web app.
- Role policies for Consultant, Manager, and Administrator.
- Object-level authorization for consultant resources and manager assignment scope.
- CSRF protection for MVC form posts.
- XSS protection through Razor encoding and no `Html.Raw` in views.
- Login lockout and audit logging through stored procedures.
- Secure cookie defaults and security headers.
- Secret-free public configuration samples.
- Prompt-injection, domain guard, AI tool allowlist, and hallucination guard coverage.

## Testing

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

Current automated suite: 240 tests across unit, integration, and security projects. The CI workflow runs restore, warning-as-error build, and tests without requiring secrets or a live SQL Server.

## Roadmap Status

Stages 0 through 19 are complete for the v1.0 release candidate. The remaining release activity is the manual real-database smoke checklist in `docs/RELEASE_CHECKLIST.md`.

## Known Limitations

- API JWT/token authentication is not implemented yet.
- Export workflows are not implemented.
- Report-run auditing is not implemented.
- Full `LeaveFlow.Web` and `LeaveFlow.Api` containerization is deferred.
- The local database setup is script-based, not migration-framework based.
- White-label settings are configuration-driven; admin CRUD and per-tenant branding are not implemented.
- Stage 16 real DB smoke remains a manual release checklist item before publishing v1.0.0.
