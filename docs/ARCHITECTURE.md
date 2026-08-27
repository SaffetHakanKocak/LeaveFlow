# Architecture

## Architecture Goals

LeaveFlow will use a maintainable, testable, loosely coupled architecture with clear boundaries between domain rules, application use cases, infrastructure services, API endpoints, and MVC UI concerns.

The architecture should stay practical. Abstractions are added only when they protect a meaningful boundary or reduce real duplication.

## Current Solution Structure

```text
src/
  LeaveFlow.Domain/
  LeaveFlow.Application/
  LeaveFlow.Infrastructure/
  LeaveFlow.Api/
  LeaveFlow.Web/

tests/
  LeaveFlow.UnitTests/
  LeaveFlow.IntegrationTests/
  LeaveFlow.SecurityTests/

db/
  001_Tables/
  002_Indexes/
  003_StoredProcedures/
  004_Seed/
  005_Security/
  006_TestData/

docs/
```

The Stage 1 solution file is `LeaveFlow.sln`.

## Project Responsibilities

### LeaveFlow.Domain

Contains core business concepts and rules that do not depend on infrastructure, ASP.NET Core, Dapper, SQL Server, or UI frameworks.

Current status: project exists and has no project references.

Planned contents:

- Domain entities
- Value objects
- Enums
- Domain services where pure domain behavior is needed
- Domain validation rules that do not require external resources

### LeaveFlow.Application

Contains use cases and application orchestration.

Planned contents:

- Commands and queries
- DTOs
- Application service interfaces
- Authorization-oriented request handling contracts
- Validation contracts
- Transaction boundary definitions

Application code may depend on Domain abstractions but must not know SQL details.

Current status: references Domain and contains dependency injection registration plus minimal data access contracts for connection creation, transactions, and role/user read repositories.

### LeaveFlow.Infrastructure

Contains integrations with external systems and persistence implementation.

Planned contents:

- Dapper stored procedure callers
- Repository implementations
- SQL Server connection factory
- Password hashing implementation
- Audit logging persistence
- Date/time provider implementation
- Email or notification infrastructure in later phases

Infrastructure may reference Application and Domain contracts.

Current status: references Application and Domain. Contains SQL Server connection factory, transaction factory, and minimal Dapper repository implementations. No Entity Framework, DbContext, generic repository, or business workflow implementation exists.

### LeaveFlow.Api

Exposes HTTP APIs for programmatic access.

Planned contents:

- Controllers or endpoint groups
- Request/response models
- API authentication and authorization policies
- API-specific validation filters
- Problem Details error responses
- OpenAPI configuration

API must enforce authorization server-side for every sensitive resource.

Current status: Web API host exists with controllers enabled, ProblemDetails, centralized exception handling, health checks, development OpenAPI, HTTPS redirection, and no business endpoints.

### LeaveFlow.Web

Provides ASP.NET Core MVC web UI.

Planned contents:

- MVC controllers
- Razor views
- View models
- Anti-forgery protection
- UI composition for consultants, managers, and administrators

The web layer must not be treated as the source of authorization truth.

Current status: MVC host exists with Home and Error shell only.

## Dependency Direction

```text
LeaveFlow.Api  ─────────────┐
LeaveFlow.Web  ─────────────┤
                            ▼
                   LeaveFlow.Application
                            ▼
                      LeaveFlow.Domain

LeaveFlow.Infrastructure ───► LeaveFlow.Application
LeaveFlow.Infrastructure ───► LeaveFlow.Domain
```

Composition root projects wire dependencies through dependency injection.

Stage 1 verified project references:

- Domain: no project references
- Application: Domain
- Infrastructure: Application, Domain
- Api: Application, Infrastructure
- Web: Application, Infrastructure

Stage 2 kept the same production project dependency direction. Test projects may reference additional projects only to validate boundaries and infrastructure behavior.

## Cross-Cutting Concerns

- Configuration through typed options
- Structured logging
- CancellationToken support on async operations
- Input validation at boundaries
- Centralized exception handling
- Audit logging for sensitive actions
- Security headers
- Secure cookie and authentication settings
- Database access through stored procedures only

## Data Access

Application code depends on interfaces in `LeaveFlow.Application`. Infrastructure implements those interfaces with Dapper and `Microsoft.Data.SqlClient`.

Current data access contracts:

- `IDbConnectionFactory`
- `IDataTransaction`
- `IDataTransactionFactory`
- `IRoleReadRepository`
- `IUserReadRepository`

Current repository implementations call stored procedures through Dapper `CommandDefinition` with `CommandType.StoredProcedure`. Stored procedure names are centralized in Infrastructure and user input is not accepted as a procedure name.

## Planned Domain Modules

- Identity and Access
- People and Organization
- Leave Management
- Approval Workflow
- Calendar and Holidays
- Availability and Timeline
- Reporting
- Audit and Compliance
- System Administration

## Non-Goals For Initial Implementation

- Multi-tenant billing
- External HR integrations
- Calendar provider sync
- Advanced workflow designer
- White-label runtime customization
- Mobile application
