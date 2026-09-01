# LeaveFlow Project Context

## Current Status

LeaveFlow has completed Stage 18: White-Label Organization Customization.

The repository now contains a .NET 10 solution with API, MVC Web, layered class library projects, working test projects, SQL Server schema scripts, stored procedure scripts, role seed script, database security guidance, a Dapper stored-procedure data layer, cookie authentication for LeaveFlow.Web, lockout and login-attempt auditing, role and object-level authorization, a deferred API authentication placeholder, administrator-facing consultant/manager management, administrator-facing organization/official holiday management, consultant self-service leave request submission/list/detail screens, manager/admin leave review with conflict detection, manager/admin workforce leave timeline views based on approved leave day rows, role-aware organization calendar views for approved leave and holidays, role-aware dashboard/reporting screens for administrators, managers, and consultants, a modern responsive MVC application shell with light/dark theme support, configuration-driven white-label branding, security hardening controls, an optional Azure AI assistant foundation, secure AI tool calling through existing application services, intelligent workforce query planning for supported LeaveFlow natural-language questions, a completed pre-v1.0 quality gate pass, and Docker SQL/public-repo setup documentation for fresh-clone local development.

API JWT/token authentication, exports, report-run auditing, full app containerization, and CI automation have not been implemented yet.

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
- Stage 9: Organization Calendar.
- Stage 10: Reporting & Management Dashboard.
- Stage 11: Professional UI/UX Pass.
- Stage 12: Security Hardening.
- Stage 13: Azure AI Assistant Foundation.
- Stage 14: Secure AI Tool Calling.
- Stage 15: Intelligent Workforce Queries.
- Stage 16: Comprehensive Test & Quality Gate.
- Stage 17: Docker & Public Repository Preparation.
- Stage 18: White-Label Organization Customization.
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
- Optional AI assistant foundation with provider-neutral Application abstractions.
- Azure AI provider implementation in Infrastructure using configuration-driven endpoint, deployment, API version, and API key.
- Authenticated `AI Asistan` Web screen with disabled feature-flag state.
- AI is disabled by default. When enabled, AI can call only registered application tools; it still has no direct SQL, repository, stored procedure, or database access.
- Stage 14 registered controlled tools for my leave requests, my leave summary, upcoming holidays, team availability, leave conflicts, upcoming leaves, and organization leave statistics.
- AI tool execution is audited without sensitive argument/content logging.
- Stage 15 added deterministic Turkish query planning for upcoming leaves, team availability, date-range conflict checks, peak leave days, request status counts, upcoming holidays, personal leave usage, and team leave summaries.
- Relative AI date handling covers today, tomorrow, this week, next week, this month, next month, and this year.
- Supported AI workforce answers use tool results as the only source for counts, dates, names, statuses, availability, conflicts, and holiday facts.
- AI domain, no-data, clarification, prompt-injection, and provider/tool failure guards are covered by unit tests.
- Role-aware organization calendar with approved leave, active organization holidays, and active official holidays.
- Calendar source-of-truth tables are `ConsultantLeaveDays`, `HolidayDays`, and `OfficialHolidayDays`.
- Consultant calendar responses include only the authenticated consultant's leave plus holidays.
- Manager calendar responses include assigned consultant leave plus holidays.
- Calendar date ranges default to month view and are capped at 62 inclusive days.
- Role-aware dashboards for administrators, managers, and consultants.
- Reporting uses `ConsultantLeaveDays` for approved leave-day analytics, `LeaveRequests` for workflow/status analytics, and holiday definition/day data for upcoming holidays.
- Managers are scoped to assigned consultants server-side and consultant users cannot access organization-wide reports.
- Reporting date ranges default to the current year and are capped at 366 inclusive days.
- Centralized CSS design tokens define typography, spacing, radius, shadows, surfaces, states, forms, buttons, badges, tables, dashboard cards, timeline, and calendar presentation.
- Authenticated MVC pages use a responsive sidebar/topbar application shell with role-aware navigation and client-side light/dark theme preference.
- UI regression tests cover shell rendering, theme toggle presence, role-aware navigation, login rendering, antiforgery token presence, and denied protected routes.
- Stage 16 quality gate completed with `dotnet clean`, `dotnet restore`, `dotnet build --no-restore`, and `dotnet test --no-build`; direct test verification reported 234 passing tests.
- Stage 16 added regression coverage for AI leap-year relative date handling and approve invalid-transition failure behavior.
- Stage 17 added Docker Compose for local SQL Server, `.env.example`, `.dockerignore`, public-repo-oriented `.gitignore` coverage, and fresh-clone setup documentation.
- `scripts/setup-local-db.ps1` remains the single local DB initialization path and applies tables, indexes, stored procedures, and seed scripts in order.
- Stage 18 added Web-layer branding options for organization name, product name, short name, optional logo URL, optional primary brand color, optional support email, and optional footer text.
- Branding is configuration-driven and generic by default. It does not add database tables, routes, admin CRUD, authorization changes, business rule changes, or AI tool changes.
- Brand color and logo values are sanitized before rendering; invalid values fall back to text/default theme behavior.

## Current Stage Scope

Stage 18 intentionally does not include:

- Direct SQL, ad hoc query, or database execution tools.
- API JWT or other token authentication
- export files
- report run auditing
- new leave business rules
- new reporting business features
- new product features beyond targeted quality fixes
- full LeaveFlow.Web or LeaveFlow.Api application containerization
- CI automation
- database-backed branding
- admin branding CRUD
- multi-tenant branding

## Next Stage

Stage 19 - Final Production & Portfolio Audit.
