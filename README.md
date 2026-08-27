# LeaveFlow

LeaveFlow is a production-minded workforce leave and calendar management platform built as an open-source portfolio project.

The project is currently in Stage 3: authentication and authorization. LeaveFlow.Web uses cookie login. Consultant CRUD, leave workflows, calendar, reporting, Azure AI, Docker, and API JWT are intentionally not implemented yet.

## Technology

- .NET 10
- ASP.NET Core Web API
- ASP.NET Core MVC
- SQL Server
- Dapper for stored procedure calls only

## Local Configuration

Do not commit real connection strings. Configure local database access with user secrets or environment variables:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=LeaveFlow;Integrated Security=true;TrustServerCertificate=true"
$env:LeaveFlow__Development__Passwords__Consultant="..."
$env:LeaveFlow__Development__Passwords__Manager="..."
$env:LeaveFlow__Development__Passwords__Administrator="..."
```

Do not commit those passwords. They are hashed at startup in Development only.

## Current Commands

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```
