# Development Log

## 2026-08-27 - Initial Planning Documentation

### Changes Made

- Created initial project planning documentation.
- Defined proposed solution architecture.
- Defined project boundaries and responsibilities.
- Identified domain modules.
- Drafted initial database entity/table list.
- Summarized initial security model.
- Created development roadmap and architectural decision log.

### Files Added

- `docs/PROJECT_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/DATABASE.md`
- `docs/SECURITY.md`
- `docs/DEVELOPMENT_LOG.md`
- `docs/ROADMAP.md`
- `docs/DECISIONS.md`

### Decisions

- Use .NET 10 with ASP.NET Core Web API and ASP.NET Core MVC.
- Use SQL Server as the primary database.
- Use Dapper only for data access.
- Require all application database access to go through stored procedures.
- Keep all SQL objects under `/db`.
- Keep the first implementation phase limited to planning documentation only.

### Test Results

No build or test commands were run in this stage because no solution, project files, or application code exist yet.

### Security Review

- No secrets, credentials, or real company-specific data were added.
- Documentation explicitly records stored procedure-only data access, backend authorization, object-level authorization, and audit logging requirements.
- No executable application surface exists yet.

### Next Step

Create the solution skeleton and empty projects, then verify with `dotnet restore`, `dotnet build`, and initial test project setup.

## 2026-08-27 - Stage 1 Solution Foundation

### Changes Made

- Created `LeaveFlow.sln` in classic `.sln` format.
- Created source projects under `src`.
- Created test projects under `tests`.
- Added repository-level `.gitignore`, `Directory.Build.props`, and initial `README.md`.
- Added Application and Infrastructure dependency injection registration extension methods.
- Configured API foundation with controllers, ProblemDetails, global exception handling, health checks, development OpenAPI, HTTPS redirection, console logging, and basic security headers.
- Configured MVC Web foundation with Home and Error pages, LeaveFlow branding, HTTPS redirection, console logging, and basic security headers.
- Removed template `Class1.cs`, `UnitTest1.cs`, WeatherForecast, and Privacy demo artifacts.

### Projects Created

- `src/LeaveFlow.Domain`
- `src/LeaveFlow.Application`
- `src/LeaveFlow.Infrastructure`
- `src/LeaveFlow.Api`
- `src/LeaveFlow.Web`
- `tests/LeaveFlow.UnitTests`
- `tests/LeaveFlow.IntegrationTests`
- `tests/LeaveFlow.SecurityTests`

### Project References

- `LeaveFlow.Domain`: no project references
- `LeaveFlow.Application`: `LeaveFlow.Domain`
- `LeaveFlow.Infrastructure`: `LeaveFlow.Application`, `LeaveFlow.Domain`
- `LeaveFlow.Api`: `LeaveFlow.Application`, `LeaveFlow.Infrastructure`
- `LeaveFlow.Web`: `LeaveFlow.Application`, `LeaveFlow.Infrastructure`
- `LeaveFlow.UnitTests`: `LeaveFlow.Domain`, `LeaveFlow.Application`
- `LeaveFlow.IntegrationTests`: `LeaveFlow.Api`
- `LeaveFlow.SecurityTests`: `LeaveFlow.Api`

### NuGet Packages

- `Microsoft.AspNetCore.OpenApi` for development OpenAPI document generation in the API project.
- `Microsoft.Extensions.DependencyInjection.Abstractions` for Application and Infrastructure DI extension methods.
- `Microsoft.Extensions.Configuration.Abstractions` for Infrastructure configuration registration contracts.
- `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, and `coverlet.collector` from the xUnit test template.
- `Microsoft.AspNetCore.Mvc.Testing` for API host integration and security tests.

### Test Results

- `dotnet --version`: `10.0.400`
- `dotnet restore`: succeeded after allowing NuGet network access.
- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors after allowing build file writes that were blocked by sandbox permissions.
- `dotnet test --no-build`: succeeded.
- Total tests: 3 passed, 0 failed, 0 skipped.

### Tests Added

- Unit test validates project reference architecture boundaries.
- Integration test validates API `/health` returns success.
- Security test validates production-style unhandled exception response does not leak internal exception details.

### Security Review

- No secrets or real company-specific data were added.
- No Entity Framework, DbContext, Dapper, SQL schema, stored procedures, repositories, authentication implementation, CORS policy, Docker, calendar, reports, or business modules were added.
- OpenAPI is development-only.
- CORS remains disabled.
- API exception responses use ProblemDetails and avoid production detail leakage.

### Next Step

Stage 2 - SQL Server + Dapper + Stored Procedure Data Layer.

## 2026-08-27 - Stage 2 SQL Server + Dapper Data Layer

### Changes Made

- Added Dapper and SQL Server driver dependencies to Infrastructure.
- Added Application data access abstractions for SQL connections and transactions.
- Added minimal role/user read repository contracts.
- Added SQL Server connection factory and transaction factory.
- Added Dapper repository implementations for roles and users.
- Added safe database configuration convention using `ConnectionStrings__DefaultConnection`.
- Created `/db` folder structure with table, index, stored procedure, seed, security, and test data files.
- Added security and unit tests for data access rules.

### Files Added

- `src/LeaveFlow.Application/Abstractions/Data/IDbConnectionFactory.cs`
- `src/LeaveFlow.Application/Abstractions/Data/IDataTransaction.cs`
- `src/LeaveFlow.Application/Abstractions/Data/IDataTransactionFactory.cs`
- `src/LeaveFlow.Application/Abstractions/Identity/IRoleReadRepository.cs`
- `src/LeaveFlow.Application/Abstractions/Identity/IUserReadRepository.cs`
- `src/LeaveFlow.Application/Identity/RoleSummary.cs`
- `src/LeaveFlow.Application/Identity/UserSummary.cs`
- `src/LeaveFlow.Infrastructure/Persistence/SqlServer/DatabaseOptions.cs`
- `src/LeaveFlow.Infrastructure/Persistence/SqlServer/StoredProcedureNames.cs`
- `src/LeaveFlow.Infrastructure/Persistence/SqlServer/SqlServerConnectionFactory.cs`
- `src/LeaveFlow.Infrastructure/Persistence/SqlServer/SqlDataTransaction.cs`
- `src/LeaveFlow.Infrastructure/Persistence/SqlServer/SqlDataTransactionFactory.cs`
- `src/LeaveFlow.Infrastructure/Persistence/Repositories/RoleReadRepository.cs`
- `src/LeaveFlow.Infrastructure/Persistence/Repositories/UserReadRepository.cs`
- `db/001_Tables/*.sql`
- `db/002_Indexes/001_CreateCoreIndexes.sql`
- `db/003_StoredProcedures/*.sql`
- `db/004_Seed/001_SeedRoles.sql`
- `db/005_Security/001_LeastPrivilegeApplicationUser.sql`
- `db/006_TestData/README.md`
- `tests/LeaveFlow.UnitTests/DatabaseConfigurationTests.cs`
- `tests/LeaveFlow.SecurityTests/DataAccessGuardTests.cs`
- `tests/LeaveFlow.SecurityTests/StoredProcedureContractTests.cs`
- `tests/LeaveFlow.IntegrationTests/DatabaseIntegrationReadinessTests.cs`

### NuGet Packages

- `Dapper` in Infrastructure for stored procedure execution and result mapping.
- `Microsoft.Data.SqlClient` in Infrastructure for SQL Server connectivity.
- `Microsoft.Extensions.Configuration` and `Microsoft.Extensions.DependencyInjection` in UnitTests for service registration tests.

### Database Objects

- Tables: Roles, Users, UserRoles, Consultants, Managers, ManagerConsultants, LeaveRequests, ConsultantLeaveDays, HolidayDefinitions, HolidayDays, OfficialHolidayDefinitions, OfficialHolidayDays, AuditLogs, LoginAttempts.
- Stored procedures: `dbo.usp_Roles_GetAll`, `dbo.usp_Users_GetById`.
- Seed: Consultant, Manager, Administrator roles.

### Test Results

- `dotnet restore`: succeeded.
- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors.
- `dotnet test --no-build`: succeeded.
- Total tests: 11 passed, 0 failed, 0 skipped.

### Security Review

- No Entity Framework, EF Core, or DbContext references in production source.
- No raw SQL statements in Application or Infrastructure C# source.
- No committed real connection string or secret.
- SQL access uses stored procedure names and `CommandType.StoredProcedure`.
- SQL Server integration tests that require a live database were not implemented in Stage 2 because no local SQL Server connection is configured.

### Next Step

Stage 3 - Authentication & Authorization.

## 2026-08-27 - Stage 3 Authentication & Authorization

### Changes Made

- Added authentication columns to `Users` and authentication stored procedures.
- Implemented cookie authentication, login/logout, lockout, and CSRF-protected MVC forms.
- Hashed passwords with ASP.NET Core Identity `PasswordHasher`.
- Added role policies and object-level consultant authorization.
- Added a deferred API authentication handler without JWT.
- Added development-only identity bootstrap from configuration.
- Added unit and security tests for login, lockout, cookies, CSRF, IDOR, and secret leakage.

### Test Results

- `dotnet restore`: succeeded.
- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors after test/compile fixes.
- `dotnet test --no-build`: succeeded. Total tests: 38 passed, 0 failed, 0 skipped (Unit 16, Security 20, Integration 2).

### Security Review

- No Entity Framework, DbContext, or raw SQL in application C# code.
- No plaintext production password, JWT secret, or connection string committed.
- Login errors stay generic. Cookies are HttpOnly and SameSite=Lax.
- Object-level authorization is enforced in application services.

### Next Step

Stage 4 - Consultant & Manager Management.

## 2026-08-27 - Stage 4 Consultant & Manager Management

### Changes Made

- Added consultant and manager management application services, validation, and repository contracts.
- Added Dapper stored-procedure repository implementations for consultant CRUD, manager CRUD, and manager-consultant assignments.
- Added SQL scripts for consultant/manager management columns, indexes, and stored procedures.
- Added administrator MVC screens for consultant and manager list/detail/create/edit/set-active workflows.
- Added manager assignment UI with duplicate assignment prevention.
- Preserved object-level authorization so consultants can view only their own profile and managers can view only assigned consultants.
- Added unit and security tests for validation, access control, CSRF, and administrator-only management.

### Test Results

- `dotnet restore`: succeeded.
- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors.
- `dotnet test --no-build`: succeeded. Total tests: 47 passed, 0 failed, 0 skipped (Unit 18, Integration 2, Security 27).

### Security Review

- No Entity Framework, DbContext, or raw SQL in application C# code.
- Management screens are protected by administrator policy except consultant detail access, which uses object-level authorization.
- MVC state-changing management actions require antiforgery tokens.
- Create/edit actions use explicit view models and application input models instead of binding persistence models.
- Database access remains stored-procedure-only through Dapper repositories.

### Next Step

Stage 5 - Holiday Management.

## 2026-08-27 - Stage 5 Organization Holiday & Official Holiday Management

### Changes Made

- Added organization holiday and official holiday application models, validation, date-range helper, services, and repository contracts.
- Added Dapper stored-procedure repositories for holiday list/detail/create/update/delete operations.
- Wrapped holiday write operations in explicit SQL transactions from the repository layer.
- Added SQL scripts for holiday date-range columns, holiday management indexes, and stored procedures.
- Added administrator MVC screens for organization holiday and official holiday list/create/edit/detail/delete workflows.
- Added organization holiday active/inactive workflow.
- Preserved Stage 4 authentication, authorization, and people management behavior.
- Did not implement consultant leave requests, approvals, calendar, timeline, reporting, Azure AI, or Docker.

### Test Results

- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors during implementation.
- `dotnet test --no-build`: succeeded. Total tests: 79 passed, 0 failed, 0 skipped (Unit 25, Integration 16, Security 38).

### Security Review

- No Entity Framework, DbContext, `CommandType.Text`, or raw SQL in application C# code.
- Holiday management endpoints are administrator-only.
- MVC state-changing holiday actions require antiforgery tokens.
- Create/edit actions use explicit view models and application input models instead of binding persistence models.
- Database access remains stored-procedure-only through Dapper repositories.

### Next Step

Stage 6 - Consultant Leave Request.

## 2026-08-27 - Stage 6 Consultant Leave Request

### Changes Made

- Added leave request status constants in Domain.
- Added consultant-scoped leave request application models, validation, service, and repository contract.
- Added Dapper stored-procedure repository implementation for create, mine list, detail, and overlap checks.
- Added SQL scripts for leave request review columns, leave request indexes, and stored procedures.
- Added Consultant-only MVC screens for My Leave Requests, New Leave Request, and Leave Request Detail.
- Ensured `ConsultantId` is resolved from the authenticated user id, not trusted from form data.
- Preserved Stage 5 holiday management and Stage 4 people management behavior.
- Did not implement manager approval, approve/reject, `ConsultantLeaveDays` generation, conflict detection, timeline, calendar, reporting, Azure AI, or Docker.

### Test Results

- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors during implementation.
- `dotnet test --no-build`: succeeded. Total tests: 96 passed, 0 failed, 0 skipped (Unit 30, Integration 21, Security 45).

### Security Review

- No Entity Framework, DbContext, `CommandType.Text`, or raw SQL in application C# code.
- Leave request screens are Consultant-only.
- MVC state-changing leave request actions require antiforgery tokens.
- Create actions use explicit view models and application input models instead of binding persistence models.
- Database access remains stored-procedure-only through Dapper repositories.

### Next Step

Stage 7 - Leave Approval & Conflict Detection.

## 2026-08-27 - Stage 7 Leave Approval & Conflict Detection

### Changes Made

- Added manager/admin leave review application models, conflict helper, validation, service, and repository methods.
- Added stored procedures for manager/admin pending queues, review detail, conflicts, approve, and reject.
- Added a unique approval-stage index for `ConsultantLeaveDays`.
- Added Manager/Admin MVC screens for Pending Leave Requests and Leave Review Detail.
- Added approve/reject POST actions with antiforgery and confirmation behavior.
- Approval now writes inclusive `ConsultantLeaveDays` rows only after a request is approved.
- Conflict preview reads approved leave day rows and excludes the current request.
- Preserved Stage 6 consultant leave request creation/list/detail behavior.
- Did not implement full workforce timeline, organization calendar, reporting/dashboard, Azure AI, or Docker.

### Test Results

- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors during implementation.
- `dotnet test --no-build`: succeeded. Total tests: 116 passed, 0 failed, 0 skipped (Unit 36, Integration 28, Security 52).

### Security Review

- No Entity Framework, DbContext, `CommandType.Text`, or raw SQL in application C# code.
- Consultants are denied from approval endpoints.
- Manager review access is scoped by assigned consultants; administrators can review all.
- Review POST actions require antiforgery tokens and explicit input models.
- Approval is concurrency-safe through pending-only locked update behavior and duplicate day-row prevention.

### Next Step

Stage 8 - Workforce Leave Timeline.

## 2026-08-27 - Stage 8 Workforce Leave Timeline

### Changes Made

- Added workforce timeline application models, validation, date header generation, matrix builder, service, and repository contract.
- Added Dapper stored-procedure repository implementation for admin and manager timeline queries.
- Added SQL scripts for admin/manager workforce timeline stored procedures.
- Added a date-first `ConsultantLeaveDays` index for timeline range reads.
- Added Manager/Admin MVC Workforce Timeline screen with date filters, consultant search, optional admin manager filter, inactive toggle, sticky consultant column, weekend/today states, leave cells, legend, empty state, and horizontal scroll.
- Preserved Stage 7 approval/conflict behavior and did not implement organization calendar, reporting/dashboard, Azure AI, Docker, or API JWT.

### Test Results

- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors during implementation.
- `dotnet test --no-build`: succeeded. Total tests: 132 passed, 0 failed, 0 skipped (Unit 42, Integration 33, Security 57).

### Security Review

- No Entity Framework, DbContext, `CommandType.Text`, or raw SQL in application C# code.
- Consultants are denied from workforce timeline access.
- Manager timeline access is scoped by assigned consultants and ignores querystring manager tampering.
- Administrator timeline access can view organization-wide approved leave and optionally filter by manager.
- Timeline data source is approved `ConsultantLeaveDays`; pending and rejected leave requests remain invisible.
- MVC timeline is read-only and uses explicit query/view models.

### Next Step

Stage 9 - Organization Calendar.

## 2026-08-28 - Stage 9 Organization Calendar

### Changes Made

- Added organization calendar application models, event type constants, date range generation, validation, event normalization, service, and repository contract.
- Added Dapper stored-procedure repository implementation for role-aware calendar list and detail queries.
- Added SQL scripts for admin, manager, and consultant organization calendar list procedures.
- Added SQL scripts for admin, manager, and consultant calendar event detail procedures.
- Added date-first indexes for organization and official holiday day range reads.
- Added authenticated MVC Organization Calendar screen with month/week view, today, previous/next, event legend, empty state, responsive calendar grid, and event detail page.
- Preserved Stage 8 workforce timeline behavior and did not implement reporting/dashboard, Azure AI, Docker, or API JWT.

### Test Results

- `dotnet build --no-restore`: succeeded with 0 warnings and 0 errors during implementation.
- `dotnet test --no-build`: succeeded. Total tests: 159 passed, 0 failed, 0 skipped (Unit 49, Integration 44, Security 66).

### Security Review

- No Entity Framework, DbContext, `CommandType.Text`, or raw SQL in application C# code.
- Calendar responses are role-aware and scoped server-side.
- Consultants cannot see other consultant names or leave details in calendar list/detail responses.
- Managers cannot see other-team leave events or details.
- Active organization and official holidays are visible to authenticated users.
- Calendar detail access prevents leave IDOR through role-specific stored procedures.

### Next Step

Stage 10 - Reporting & Dashboard.
