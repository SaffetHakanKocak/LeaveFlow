# LeaveFlow Project Context

## Current Status

LeaveFlow has completed Stage 2: SQL Server + Dapper + Stored Procedure Data Layer.

The repository now contains a .NET 10 solution with API, MVC Web, layered class library projects, working test projects, SQL Server schema scripts, stored procedure scripts, role seed script, database security guidance, and a minimal Dapper-based data access foundation.

Business workflows, authentication implementation, login UI, consultant CRUD, leave approval, timeline, calendar, reporting, Azure AI, and Docker have not been implemented yet.

This repository is intended to become a production-grade, open-source portfolio project for workforce leave and calendar management.

## Product Purpose

LeaveFlow will support:

- Employee/consultant leave requests
- Manager approval workflows
- Organization holidays and public holidays
- Team availability and leave conflict visibility
- Timeline and organization calendar views
- Reporting for administrators and managers

The project is generic and white-label friendly. It must not contain real company names, logos, private data, or organization-specific assumptions.

## Technology Stack

- Backend: .NET 10
- API: ASP.NET Core Web API
- Web: ASP.NET Core MVC
- Database: Microsoft SQL Server
- Data access: Dapper only
- Database access rule: application code calls stored procedures only
- Testing: unit, integration, and security test projects

## Critical Rules

- Do not use Entity Framework, Entity Framework Core, DbContext, or ORM-based LINQ-to-database patterns.
- Do not place raw SQL queries in application code.
- All application database operations must call stored procedures.
- SQL scripts must live under `/db`.
- Secrets, API keys, real credentials, and real user data must not be committed.
- Authorization must be enforced on the backend, including object-level authorization.
- Do not implement future phases before they are explicitly requested.

## Completed Stages

- Stage 0: Initial technical planning documentation.
- Stage 1: .NET solution foundation.
- Stage 2: SQL Server + Dapper + Stored Procedure Data Layer.
- Initial technical planning documentation created.
- Proposed solution architecture documented.
- Folder structure and project boundaries defined.
- Initial domain modules identified.
- Initial database entity/table list drafted.
- Initial security model summarized.
- Development roadmap drafted.
- `LeaveFlow.sln` created with `src` and `tests` projects.
- API foundation created with ProblemDetails, global exception handling, health checks, development OpenAPI endpoint, HTTPS redirection, and environment-aware middleware.
- MVC Web foundation created with a basic LeaveFlow shell.
- Unit, integration, and security smoke tests added.
- `/db` structure created with tables, indexes, stored procedures, seed, security guidance, and test data guidance.
- Infrastructure data access foundation created with Dapper and Microsoft.Data.SqlClient.
- Minimal read repositories added for roles and users using stored procedure calls only.
- Transaction abstraction and SQL Server transaction factory added for future workflows.

## Current Stage Scope

Stage 2 intentionally does not include:

- Business endpoints
- Consultant, manager, leave, calendar, or reporting screens
- Business logic
- Authentication implementation
- CI/CD implementation
- Login UI
- Password hashing implementation
- Docker

## Next Stage

Stage 3 - Authentication & Authorization.
