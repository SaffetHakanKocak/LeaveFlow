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
