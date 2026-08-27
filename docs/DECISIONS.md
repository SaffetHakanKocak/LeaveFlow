# Decisions

## ADR-0001 - Use Layered Solution Structure

Date: 2026-08-27

Status: accepted

### Context

LeaveFlow needs to be maintainable, testable, and suitable as a production-grade portfolio project.

### Decision

Use separate projects for Domain, Application, Infrastructure, API, and Web layers.

### Consequences

- Business concepts can remain independent from infrastructure and UI concerns.
- Tests can target domain and application behavior cleanly.
- More project boundaries must be maintained.

## ADR-0002 - Use Dapper Only For Data Access

Date: 2026-08-27

Status: accepted

### Context

The project requires SQL Server access without Entity Framework or ORM-based database access.

### Decision

Use Dapper as the only data access library.

### Consequences

- SQL behavior stays explicit.
- Stored procedure contracts become important application boundaries.
- Developers must avoid raw SQL in application code.

## ADR-0003 - Require Stored Procedures For Application Database Access

Date: 2026-08-27

Status: accepted

### Context

The project requires all application database operations to go through stored procedures.

### Decision

Application code must call stored procedures such as `dbo.usp_LeaveRequests_GetForConsultant` instead of embedding SQL statements.

### Consequences

- Database access can be permissioned through execute-only rights.
- SQL Server scripts must be versioned and reviewed carefully.
- Integration tests should validate stored procedure behavior.

## ADR-0004 - Treat Authorization As Backend Responsibility

Date: 2026-08-27

Status: accepted

### Context

Leave data is sensitive. UI-only authorization is insufficient and creates IDOR risk.

### Decision

Every sensitive resource must be authorized in backend code. Stored procedures should also support scoped access where practical.

### Consequences

- Consultants cannot access other consultants' private leave data through direct id manipulation.
- Managers must be scoped to assigned consultants and teams.
- Tests must include object-level authorization cases.

## ADR-0005 - Keep Initial Scope To Documentation Only

Date: 2026-08-27

Status: accepted

### Context

The first requested stage is planning only. Business logic and application modules are explicitly out of scope.

### Decision

Create planning documentation and stop before generating application code.

### Consequences

- The repository now has a durable context system for future phases.
- No build or test commands are meaningful until solution files exist.

## ADR-0006 - Use Repository-Level Build Defaults

Date: 2026-08-27

Status: accepted

### Context

All projects should share consistent .NET compiler defaults without repeating decisions manually in every future project.

### Decision

Add `Directory.Build.props` with `net10.0`, nullable reference types, implicit usings, deterministic builds, and CI reproducibility support. `TreatWarningsAsErrors` remains disabled for local Debug builds at this stage, while warnings are still reviewed and fixed.

### Consequences

- Future projects inherit the same baseline.
- CI can opt into reproducible build metadata.
- Warning policy can be tightened later once the foundation stabilizes.

## ADR-0007 - Defer Dapper Until Database Foundation

Date: 2026-08-27

Status: accepted

### Context

Stage 1 creates the solution foundation only. Database schema, stored procedures, and repositories are out of scope.

### Decision

Do not add Dapper in Stage 1. Add it in Stage 2 when SQL Server stored procedure data access is implemented.

### Consequences

- Infrastructure remains minimal.
- No unused data access package is introduced.
- The stored procedure-only rule remains documented and ready for Stage 2.

## ADR-0008 - Use Stored Procedure Specific Repositories

Date: 2026-08-27

Status: accepted

### Context

The data layer needs to prove SQL Server + Dapper access without creating broad CRUD abstractions or leaking SQL into Application.

### Decision

Add minimal role and user read repository contracts and implementations. Repositories call centralized stored procedure names with `CommandType.StoredProcedure`.

### Consequences

- Data access remains use-case oriented.
- No generic repository abstraction is introduced.
- Future repositories should follow the same stored procedure-only pattern.

## ADR-0009 - Defer Refresh Tokens Until Authentication Design

Date: 2026-08-27

Status: accepted

### Context

Stage 2 includes database foundation, but the exact authentication model is not yet selected.

### Decision

Add `LoginAttempts` for future brute force protection, but do not add `RefreshTokens` until Stage 3 defines token/session strategy.

### Consequences

- The schema avoids a premature token model.
- Authentication storage can be designed around the chosen auth mechanism.
