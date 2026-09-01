# Contributing

Thanks for helping improve LeaveFlow.

## Development Setup

1. Install the .NET 10 SDK.
2. Clone the repository.
3. Copy `.env.example` to `.env` for local SQL Server and demo-user settings.
4. Start SQL Server with Docker Compose if database-backed manual testing is needed.
5. Run the database setup script before using the MVC app locally.

## Quality Gate

Before opening a pull request, run:

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

Builds should complete with 0 warnings and tests should pass without requiring secrets or a live SQL Server.

## Architecture Rules

- Keep domain code independent of infrastructure and UI concerns.
- Keep application database access behind abstractions.
- Use Dapper stored-procedure repositories for SQL Server access.
- Do not introduce Entity Framework, `DbContext`, raw SQL in C# source, or `CommandType.Text`.
- Preserve backend authorization and object-level access checks.
- Keep AI tool calling read-only, allowlisted, scoped, and audited.

## Security And Privacy

- Do not commit real passwords, API keys, connection strings, user data, private endpoints, or private logos/assets.
- Use `.env`, user secrets, or local environment variables for secrets.
- Keep demo users development-only.
- Add regression tests for authorization, IDOR, CSRF, XSS, AI guard, and date boundary changes.

## Pull Requests

- Keep changes focused.
- Include tests for behavior changes.
- Update documentation when setup, security, architecture, or release behavior changes.
- Do not include unrelated formatting churn or generated artifacts.
