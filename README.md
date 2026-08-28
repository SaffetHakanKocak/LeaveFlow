# LeaveFlow

LeaveFlow is a production-minded workforce leave and calendar management platform built as an open-source portfolio project.

## Technology

- .NET 10
- ASP.NET Core Web API
- ASP.NET Core MVC
- SQL Server
- Dapper for stored procedure calls only

## Local Configuration

Do not commit real connection strings. Configure local database access with user secrets or environment variables:

```powershell
$env:LEAVEFLOW_SQL_PASSWORD="<your-sql-sa-password>"
$env:LeaveFlow__Development__DemoPassword="<your-demo-user-password>"
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=LeaveFlow;User Id=sa;Password=$env:LEAVEFLOW_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True"
```

Do not commit those passwords. Demo user passwords are hashed at startup in Development only.

### Local SQL Server Setup

Start SQL Server in Docker, then apply the database scripts in dependency order:

```powershell
docker run --name leaveflow-sql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=$env:LEAVEFLOW_SQL_PASSWORD" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
.\scripts\setup-local-db.ps1
```

The setup script creates the `LeaveFlow` database when missing and applies:

```text
db/001_Tables -> db/002_Indexes -> db/003_StoredProcedures -> db/004_Seed
```

In Development, LeaveFlow bootstraps these demo users when `LeaveFlow:Development:BootstrapIdentity` is enabled:

```text
admin@leaveflow.local
manager@leaveflow.local
consultant1@leaveflow.local
consultant2@leaveflow.local
```

The bootstrap is idempotent and keeps the demo consultants assigned to the demo manager.

Run the web app:

```powershell
dotnet run --project src\LeaveFlow.Web\LeaveFlow.Web.csproj
```

## Current Commands

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```
