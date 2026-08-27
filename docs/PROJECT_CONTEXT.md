# LeaveFlow Project Context

## Current Status

LeaveFlow is at the initial planning stage. No application modules, business logic, database objects, or UI screens have been implemented yet.

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

- Initial technical planning documentation created.
- Proposed solution architecture documented.
- Folder structure and project boundaries defined.
- Initial domain modules identified.
- Initial database entity/table list drafted.
- Initial security model summarized.
- Development roadmap drafted.

## Current Stage Scope

This stage intentionally does not include:

- .NET solution creation
- API endpoints
- MVC screens
- Business logic
- Database scripts
- Authentication implementation
- CI/CD implementation

## Next Stage

Recommended next stage: create the solution skeleton, project files, test projects, shared build configuration, and placeholder folder structure without implementing business workflows.
