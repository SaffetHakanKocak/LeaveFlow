# LeaveFlow Project Context

## Current Status

LeaveFlow has completed Stage 8: Workforce Leave Timeline.

The repository now contains a .NET 10 solution with API, MVC Web, layered class library projects, working test projects, SQL Server schema scripts, stored procedure scripts, role seed script, database security guidance, a Dapper stored-procedure data layer, cookie authentication for LeaveFlow.Web, lockout and login-attempt auditing, role and object-level authorization, a deferred API authentication placeholder, administrator-facing consultant/manager management, administrator-facing organization/official holiday management, consultant self-service leave request submission/list/detail screens, manager/admin leave review with conflict detection, and manager/admin workforce leave timeline views based on approved leave day rows.

Organization calendar, reporting, Azure AI, Docker, and API JWT/token authentication have not been implemented yet.

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
- Stage 3: Authentication & Authorization.
- Stage 4: Consultant & Manager Management.
- Stage 5: Organization Holiday & Official Holiday Management.
- Stage 6: Consultant Leave Request.
- Stage 7: Leave Approval & Conflict Detection.
- Stage 8: Workforce Leave Timeline.
- Cookie authentication for LeaveFlow.Web.
- Password hashing with ASP.NET Core Identity `PasswordHasher`.
- Configurable lockout and login-attempt auditing.
- Role-based policies and object-level consultant access checks.
- CSRF protection for MVC form POSTs, including login and logout.
- Development-only identity bootstrap from configuration, without committed passwords.
- Consultant and manager list/detail/create/edit/set-active screens for administrators.
- Manager-to-consultant assignment management with duplicate prevention.
- Consultant self-profile access and manager assigned-consultant detail access.
- Organization holiday and official holiday admin screens.
- Inclusive day-row generation for holiday date ranges.
- Transaction-wrapped holiday create/update/delete repository operations.
- Consultant leave request create/list/detail workflow with backend consultant scoping.
- Pending-only request creation with duplicate/overlap protection.
- Manager/Admin pending leave review workflow.
- Conflict preview based on approved `ConsultantLeaveDays`.
- Concurrency-safe approve/reject operations with approved day-row generation.
- Manager/Admin workforce timeline with one row per consultant and one column per day.
- Timeline source of truth is approved `ConsultantLeaveDays`.
- Admin timeline can optionally filter by manager; manager timeline is scoped to assigned consultants server-side.
- Timeline date range defaults to the current month and is capped at 62 inclusive days.

## Current Stage Scope

Stage 8 intentionally does not include:

- Organization calendar
- Reporting/dashboard
- Azure AI
- Docker
- API JWT or other token authentication

## Next Stage

Stage 9 - Organization Calendar.
