# LeaveFlow

LeaveFlow is a production-minded workforce leave and calendar management platform built as an open-source portfolio project. It covers leave requests, manager approvals, team availability, organization calendars, reporting, and an optional guarded AI assistant.

## Architecture

- `LeaveFlow.Domain`: framework-independent domain constants and rules.
- `LeaveFlow.Application`: use cases, validation, authorization-aware orchestration, reporting, timeline, calendar, and AI tool boundaries.
- `LeaveFlow.Infrastructure`: SQL Server, Dapper stored-procedure repositories, password hashing, audit persistence, and Azure AI provider integration.
- `LeaveFlow.Web`: ASP.NET Core MVC browser UI.
- `LeaveFlow.Api`: API host foundation with protected sample endpoint and health checks.

Database access is Dapper + stored procedures only. Application code must not use Entity Framework, `DbContext`, raw SQL, or generic query execution tools.

## Tech Stack

- .NET 10
- ASP.NET Core MVC and Web API
- SQL Server 2022
- Dapper
- xUnit test projects for unit, integration, and security coverage
- Docker Compose for local SQL Server

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

Edit `.env` and set strong local values for:

```text
LEAVEFLOW_SQL_PASSWORD
LeaveFlow__Development__DemoPassword
```

Start SQL Server:

```powershell
docker compose up -d sqlserver
```

If your Docker installation uses the older standalone Compose binary, use:

```powershell
docker-compose up -d sqlserver
```

Load the `.env` values into the current PowerShell session:

```powershell
Get-Content .env | ForEach-Object {
    if ($_ -match '^\s*#' -or $_ -notmatch '=') { return }
    $name, $value = $_ -split '=', 2
    [Environment]::SetEnvironmentVariable($name.Trim(), $value.Trim(), 'Process')
}

$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=LeaveFlow;User Id=sa;Password=$env:LEAVEFLOW_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True"
```

Initialize the database with the existing setup script:

```powershell
.\scripts\setup-local-db.ps1 -DockerContainer leaveflow-sql
```

Run the web app:

```powershell
dotnet run --project src\LeaveFlow.Web\LeaveFlow.Web.csproj
```

Open the local URL shown by `dotnet run`, commonly:

```text
http://localhost:5226
```

## Database Initialization

`scripts/setup-local-db.ps1` is the single local database initialization path. It creates the `LeaveFlow` database when missing and applies scripts in this order:

```text
db/001_Tables
db/002_Indexes
db/003_StoredProcedures
db/004_Seed
```

Do not add a second database initialization system unless the architecture is explicitly changed later.

## Development Demo Users

When `ASPNETCORE_ENVIRONMENT=Development`, `LeaveFlow:Development:BootstrapIdentity=true`, and a demo password is configured, the web app creates/updates these local demo accounts:

```text
admin@leaveflow.local
manager@leaveflow.local
consultant1@leaveflow.local
consultant2@leaveflow.local
```

The demo password is hashed at startup and is never stored in source, SQL scripts, or committed config. The bootstrap also assigns both demo consultants to the demo manager.

## Azure AI Configuration

AI is disabled by default and LeaveFlow runs normally without Azure AI credentials.

To enable it locally, provide values through environment variables or user secrets:

```powershell
$env:AI__Enabled="true"
$env:AI__Provider="Azure"
$env:AI__Azure__Endpoint="https://<your-resource>.openai.azure.com"
$env:AI__Azure__Deployment="<your-deployment>"
$env:AI__Azure__ApiVersion="2024-02-15-preview"
$env:AI__Azure__ApiKey="<your-api-key>"
```

Do not commit AI keys. The AI assistant can only use allowlisted read-only LeaveFlow tools and cannot execute SQL or bypass backend authorization.

## Security Architecture

- Cookie authentication for the MVC web app.
- Role policies for Consultant, Manager, and Administrator.
- Object-level authorization for consultant resources and manager assignments.
- CSRF protection for MVC form posts.
- Password hashing with ASP.NET Core Identity password hasher.
- Login attempt and audit logging through stored procedures.
- Dapper stored-procedure-only data access.
- AI feature flag, safe provider failures, bounded tool calling, tool audit metadata, and domain/hallucination guards.

## Testing

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

Stage 16 quality gate currently verifies unit, integration, and security coverage. A real database smoke test should be run manually against the Docker SQL Server and local Web instance before release.

## Known Limitations

- API JWT/token authentication is not implemented yet.
- Export workflows are not implemented.
- CI and public repository automation are deferred until repository publication work.
- The local database setup is script-based, not migration-framework based.
- Azure AI is optional and disabled by default.
