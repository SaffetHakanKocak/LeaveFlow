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

## ADR-0010 - Cookie Authentication For Web, Deferred API Tokens

Date: 2026-08-27

Status: accepted

### Context

Stage 3 requires secure authentication. LeaveFlow.Web is an MVC browser app. LeaveFlow.Api should remain ready for tokens later without installing JWT now.

### Decision

Use ASP.NET Core cookie authentication for LeaveFlow.Web. Register a deferred API authentication scheme that never authenticates and challenges with 401 ProblemDetails. Do not add RefreshTokens or JWT packages in this stage.

### Consequences

- Browser sessions use a server-issued cookie with HttpOnly, SameSite=Lax, and environment-aware Secure flags.
- API callers cannot log in yet. Protected API endpoints return 401.
- Sliding cookie expiration (8 hours by default) is used because this is an internal workforce web app, not a public SPA.

## ADR-0011 - ASP.NET Identity PasswordHasher And Configurable Lockout

Date: 2026-08-27

Status: accepted

### Context

Passwords must not be stored in plaintext, and a custom hash algorithm is forbidden.

### Decision

Hash and verify passwords with `PasswordHasher<object>` from ASP.NET Core Identity. Apply lockout after a configured number of failed attempts for a configured duration. Record every attempt in `LoginAttempts` and security events in `AuditLogs`.

### Consequences

- Hash format follows Identity V3 PBKDF2.
- Lockout thresholds are not magic numbers in code.
- Failed logins never disclose whether the email exists, is inactive, or is locked.

## ADR-0012 - Application-Layer Object Authorization

Date: 2026-08-27

Status: accepted

### Context

Consultants and managers must not access another team's resources through identifier guessing. UI hiding is insufficient.

### Decision

Evaluate consultant resource access in `IConsultantResourceAuthorizationService` using role names, consultant identity, and manager assignment stored procedures. Administrators are allowed. Controllers call this service before returning a resource.

### Consequences

- Object-level rules are unit-testable without SQL Server.
- Stored procedures remain the data source for identities and assignments.
- Leave request authorization in later stages should reuse the same evaluator.

## ADR-0013 - Holiday Day Rows Are Regenerated On Update

Date: 2026-08-27

Status: accepted

### Context

Holiday definitions own generated day rows for inclusive date ranges. Updating a date range can leave stale child rows if changes are applied incrementally.

### Decision

For holiday updates, update the definition, delete the existing day rows, and regenerate the full inclusive day range in one transaction.

### Consequences

- Parent and child rows remain consistent.
- The stored procedures stay straightforward and auditable.
- Update operations rewrite child day rows even when only the name changes.

## ADR-0014 - Holiday Deletes Remove Child Rows First

Date: 2026-08-27

Status: accepted

### Context

Holiday day rows reference holiday definitions. Delete behavior must preserve referential integrity without relying on hidden cascade behavior.

### Decision

Delete child day rows first, then delete the definition row, inside the same transaction-wrapped repository operation.

### Consequences

- Referential integrity is explicit in reviewed stored procedure scripts.
- No broad cascade delete setting is required.
- Future tables referencing holiday definitions must be reviewed before delete behavior changes.

## ADR-0015 - Holiday Overlaps Are Allowed, Exact Duplicates Are Rejected

Date: 2026-08-27

Status: accepted

### Context

Organization holidays and official holidays may legitimately overlap. For example, an internal company closure can overlap an official public holiday.

### Decision

Reject exact duplicates by name plus inclusive date range. Allow partial and nested overlaps between different holiday definitions.

### Consequences

- Administrators can model real-world overlapping holidays.
- Duplicate records for the same holiday range are blocked.
- Reporting or availability logic in later stages must decide how to combine overlapping holiday days.

## ADR-0016 - Reject Past-Dated New Leave Requests

Date: 2026-08-27

Status: accepted

### Context

Consultants can create their own leave requests in Stage 6. Past-dated requests can create approval ambiguity and retrospective payroll or availability issues.

### Decision

Reject new leave requests whose start date is before the current application date.

### Consequences

- Consultants must submit requests before the leave period begins.
- Retrospective corrections require a future administrative workflow rather than self-service creation.
- Tests pin the validation behavior through an injectable `TimeProvider`.

## ADR-0017 - Leave Request Overlaps Block Pending And Approved Requests

Date: 2026-08-27

Status: accepted

### Context

A consultant should not create overlapping leave requests that could later be approved into an inconsistent schedule. Rejected requests should not prevent resubmission.

### Decision

For the same consultant, reject exact duplicates and partial or nested overlaps against existing `Pending` or `Approved` requests. Ignore `Rejected` requests in overlap checks.

### Consequences

- Duplicate and overlapping pending work is blocked at the backend/database boundary.
- Consultants can resubmit after rejection.
- `ConsultantLeaveDays` remains reserved for approval/conflict detection in Stage 7.

## ADR-0018 - Approval Uses Pending-Only Locked Update And Unique Day Rows

Date: 2026-08-27

Status: accepted

### Context

The same pending leave request can be opened in multiple tabs or processed by repeated requests. Approval must not create duplicate approved state or duplicate leave day rows.

### Decision

Approve and reject stored procedures load the request with update locks, require `Status = Pending`, validate reviewer scope, then perform a pending-only update inside a database transaction. Approval inserts one `ConsultantLeaveDays` row per inclusive date after the status update succeeds. A unique index on `ConsultantId`, `LeaveRequestId`, and `LeaveDate` prevents duplicate day rows.

### Consequences

- Only the first concurrent approval/rejection can succeed.
- Repeated approve/reject and approve-after-reject/reject-after-approve fail safely.
- Approved leave day rows are reliable input for conflict detection and future timeline work.

## ADR-0019 - Conflicts Are Decision Support, Not Automatic Approval Blocks

Date: 2026-08-27

Status: accepted

### Context

Managers and administrators need visibility into overlapping approved leave, but some teams may intentionally approve overlapping absences.

### Decision

Conflict detection returns approved leave day overlaps for the requested date range and excludes the current request. The UI shows a clear warning and preview, but approval is not automatically blocked by conflicts.

### Consequences

- Reviewers retain business discretion.
- Conflict data is visible before approval.
- Future policy automation can add stricter blocking rules without changing the basic review model.

## ADR-0020 - Workforce Timeline Uses Approved Leave Day Rows

Date: 2026-08-27

Status: accepted

### Context

The workforce timeline needs a reliable day-level availability source without duplicating leave status rules in the UI.

### Decision

Build the workforce timeline from `ConsultantLeaveDays` and guard joins with approved `LeaveRequests`. Pending and rejected leave requests are not timeline inputs.

### Consequences

- Approval remains the only path that creates visible leave day cells.
- Rejected and pending requests stay out of team availability views.
- Timeline and conflict detection share the same approved day-row source of truth.

## ADR-0021 - Cap Workforce Timeline Date Range At 62 Days

Date: 2026-08-27

Status: accepted

### Context

A row-per-consultant and column-per-day timeline can become wide and expensive when arbitrary ranges are allowed.

### Decision

Default the timeline to the current month and cap requested ranges at 62 inclusive days through application validation.

### Consequences

- Normal month and two-month planning views are supported.
- Very large date ranges are rejected before querying or rendering.
- Wider planning/reporting needs should be handled by a future reporting/export feature.

## ADR-0022 - Resolve Workforce Timeline Scope Server-Side

Date: 2026-08-27

Status: accepted

### Context

Manager timeline access is sensitive because querystring parameters can be tampered with.

### Decision

Managers never control their effective manager scope through request parameters. The application service resolves the manager id from the authenticated user and calls the manager-scoped stored procedure with that id. Administrators may use an optional manager filter.

### Consequences

- Manager `managerId` tampering is ignored.
- Database queries remain scoped through `ManagerConsultants`.
- The UI is not the source of authorization truth.

## ADR-0023 - Organization Calendar Uses Unified Events Without A Client Calendar Dependency

Date: 2026-08-28

Status: accepted

### Context

The organization calendar needs month/week views, role-aware server data, and open-source friendly implementation without adding unnecessary frontend weight.

### Decision

Use a server-rendered Razor/Bootstrap calendar grid instead of adding FullCalendar or another client calendar package in this stage. Normalize day-level database rows into a unified `CalendarEvent` model in the application layer.

### Consequences

- No new frontend package, CDN dependency, or offline production concern is introduced.
- Calendar rendering stays simple and testable for the current scope.
- A richer client calendar library can still be evaluated later if drag/drop, external sync, or complex recurrence is required.

## ADR-0024 - Organization Calendar Scope Is Role-Aware At The Backend

Date: 2026-08-28

Status: accepted

### Context

Calendar responses can leak sensitive leave visibility if filtering is done only in the browser or trusted from querystring parameters.

### Decision

Use separate role-aware calendar stored procedures for administrator, manager, and consultant scopes. The application service resolves the current actor's manager or consultant id server-side and ignores tampered scope parameters for non-admin users.

### Consequences

- Consultants receive only their own leave plus holidays.
- Managers receive only assigned consultant leave plus holidays.
- Administrators can use optional manager/consultant filters.
- Calendar list and detail endpoints share the same backend authorization principle.

## ADR-0025 - Cap Organization Calendar Date Range At 62 Days

Date: 2026-08-28

Status: accepted

### Context

The calendar combines leave and holiday day rows and can become expensive or visually noisy for arbitrary date ranges.

### Decision

Default organization calendar to month view and cap requested ranges at 62 inclusive days, matching the workforce timeline approach.

### Consequences

- Month and two-month planning windows are supported.
- Large reporting-style queries are deferred to the future reporting/dashboard stage.
- Calendar UI stays responsive and predictable.

## ADR-0026 - Cap Reporting Date Range At 366 Days

Date: 2026-08-28

Status: accepted

### Context

Reports aggregate leave days, request statuses, team usage, peak dates, and upcoming holidays. Arbitrary multi-year ranges can produce expensive queries and noisy screens before export/background report execution exists.

### Decision

Default report filters to the current calendar year and cap requested ranges at 366 inclusive days through application validation.

### Consequences

- Normal annual reporting is supported.
- Very large reporting windows are rejected before querying or rendering.
- Multi-year analytics should be handled by a future export/background reporting design.

## ADR-0027 - Resolve Reporting Scope Server-Side

Date: 2026-08-28

Status: accepted

### Context

Reporting can expose organization-wide leave and staffing information. Querystring `managerId` or `consultantId` values must not be trusted for non-admin users.

### Decision

Administrators may run organization reports with optional filters. Managers have their effective manager id resolved from the authenticated user and supplied filters cannot expand scope. Consultants are denied organization-wide reports and receive only a personal dashboard.

### Consequences

- Manager filter tampering is ignored.
- Consultant users cannot access organization analytics.
- Stored procedures still receive scoped parameters so database reads align with application authorization.

## ADR-0028 - Keep Reporting UI Server-Rendered Without A Chart Dependency

Date: 2026-08-28

Status: accepted

### Context

Stage 10 needs practical dashboard and report screens without introducing frontend package management or CDN availability concerns.

### Decision

Use Razor/Bootstrap summary cards and tables for the reporting UI in this stage. Do not add Chart.js or another chart dependency yet.

### Consequences

- No new frontend license, package, or CDN dependency is introduced.
- Report data remains visible, filterable, and testable with server-rendered HTML.
- Rich charts can be evaluated in a later professional UI/UX or export/reporting phase.

## ADR-0029 - Use CSS Design Tokens For The MVC UI

Date: 2026-08-28

Status: accepted

### Context

The MVC UI needed a professional pass without changing business workflows or introducing a large frontend architecture.

### Decision

Centralize the visual language in `site.css` using CSS custom properties for typography, spacing, radius, shadows, surfaces, text colors, interaction states, forms, tables, badges, dashboard cards, calendar, and timeline styling.

### Consequences

- Existing Razor views can be polished without controller/service rewrites.
- Light and dark mode can share the same component classes.
- Future UI work has a clear token layer instead of scattered hard-coded colors.

## ADR-0030 - Keep The Professional UI Pass On Existing Bootstrap Assets

Date: 2026-08-28

Status: accepted

### Context

Stage 11 allowed lightweight open-source UI/icon assets but did not require a new dependency. The project already vendors Bootstrap, jQuery, and jQuery Validation template assets with their licenses.

### Decision

Use the existing Bootstrap assets and custom CSS/JavaScript for the application shell, responsive behavior, and theme toggle. Do not add Bootstrap Icons, Chart.js, CDNs, or a new frontend package in this stage.

### Consequences

- No new third-party license, package restore, CDN, or offline production concern is introduced.
- UI polish remains server-rendered and easy to test through MVC security tests.
- A dedicated icon library can be evaluated later if the product needs richer iconography.
