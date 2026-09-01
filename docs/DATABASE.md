# Database

## Database Principles

LeaveFlow uses Microsoft SQL Server. Application code must access the database only through stored procedures using Dapper.

Rules:

- No Entity Framework, EF Core, DbContext, or ORM-style database access.
- No raw SQL statements in Application or Infrastructure C# code.
- Dapper is used only to execute stored procedures and map results.
- Stored procedure calls must use `CommandType.StoredProcedure`.
- SQL scripts live under `/db`.
- Real connection strings and secrets must not be committed.

## Folder Structure

```text
db/
  001_Tables/
  002_Indexes/
  003_StoredProcedures/
  004_Seed/
  005_Security/
  006_TestData/
```

## Script Execution Order

For a fresh development database:

1. Create the SQL Server database outside this repository.
2. Run scripts in `db/001_Tables` in numeric order.
3. Run scripts in `db/002_Indexes` in numeric order.
4. Run scripts in `db/003_StoredProcedures` in numeric order.
5. Run scripts in `db/004_Seed` in numeric order.
6. Review and adapt scripts in `db/005_Security` for the target environment.
7. Use `db/006_TestData` only for fictional local test data.

For local Docker development, start SQL Server with `docker compose up -d sqlserver`, then run `scripts/setup-local-db.ps1`. The setup script remains the single initialization path; Docker Compose only provides the SQL Server process and persistent local volume.

No migration framework is used. New authentication columns for existing databases are added by `db/001_Tables/015_AlterUsersAddAuthenticationColumns.sql`. Stage 4 people-management columns for existing consultant and manager tables are added by `db/001_Tables/016_AlterConsultantsAddManagementColumns.sql` and `db/001_Tables/017_AlterManagersAddManagementColumns.sql`. Stage 5 holiday date ranges are added by `db/001_Tables/018_AlterHolidayDefinitionsAddDateRange.sql` and `db/001_Tables/019_AlterOfficialHolidayDefinitionsAddDateRange.sql`. Stage 6 review columns for leave requests are added by `db/001_Tables/020_AlterLeaveRequestsAddReviewColumns.sql`.

## Tables

- `Roles`: system roles such as Consultant, Manager, and Administrator.
- `Users`: identity account including email, password hash, active flag, failed-login count, lockout end, and last login timestamp.
- `UserRoles`: many-to-many assignment between users and roles.
- `Consultants`: consultant profile root linked to a user, including profile names, department, start date, and active state.
- `Managers`: manager profile root linked to a user, including profile names, department, start date, and active state.
- `ManagerConsultants`: manager-to-consultant assignment scope.
- `LeaveRequests`: consultant leave request header data with reason, date range, status, created timestamp, and deferred review fields.
- `ConsultantLeaveDays`: per-day leave expansion for availability and conflict checks.
- `HolidayDefinitions`: organization holiday grouping with name, inclusive date range, and active state.
- `HolidayDays`: organization holiday dates.
- `OfficialHolidayDefinitions`: official holiday grouping by country and optional region with name and inclusive date range.
- `OfficialHolidayDays`: official holiday dates.
- `AuditLogs`: security and business audit trail.
- `LoginAttempts`: authentication attempt audit used for brute-force review.

`RefreshTokens` is still not added. Stage 3 uses cookie sessions for the web host and does not issue API tokens.

## Relationship Summary

- `UserRoles.UserId` references `Users.Id`.
- `UserRoles.RoleId` references `Roles.Id`.
- `Consultants.UserId` references `Users.Id` and is unique.
- `Managers.UserId` references `Users.Id` and is unique.
- `ManagerConsultants.ManagerId` references `Managers.Id`.
- `ManagerConsultants.ConsultantId` references `Consultants.Id`.
- `LeaveRequests.ConsultantId` references `Consultants.Id`.
- `ConsultantLeaveDays.LeaveRequestId` references `LeaveRequests.Id`.
- `ConsultantLeaveDays.ConsultantId` references `Consultants.Id`.
- `HolidayDays.HolidayDefinitionId` references `HolidayDefinitions.Id`.
- `OfficialHolidayDays.OfficialHolidayDefinitionId` references `OfficialHolidayDefinitions.Id`.
- `AuditLogs.ActorUserId` references `Users.Id`.
- `LoginAttempts.UserId` references `Users.Id`.

## Indexing Approach

Unique constraints protect natural uniqueness for role names, normalized email, consultant user id, manager user id, and holiday definition scopes.

Indexes were added for likely lookup paths:

- role-to-user joins
- consultant manager assignment lookup
- leave requests by consultant, status, and date range
- leave days by consultant and date
- workforce timeline reads by leave date and consultant
- holiday days by date
- audit logs by actor, target, and timestamp
- login attempts by normalized email and timestamp
- consultant and manager management lists by active state and name
- holiday management lists by active state, name, and date range
- consultant leave requests by consultant, status, and date range
- reporting reads by leave request status, date range, and consultant

Indexes should be revisited when real stored procedure query patterns are implemented.

## Stored Procedures

Naming convention:

```text
dbo.usp_<Module>_<Action>
```

Current stored procedures:

- `dbo.usp_Roles_GetAll`
- `dbo.usp_Users_GetById`
- `dbo.usp_Users_GetByNormalizedEmail`
- `dbo.usp_Users_UpdateLoginSuccess`
- `dbo.usp_Users_RecordFailedLogin`
- `dbo.usp_Users_UpdatePasswordHash`
- `dbo.usp_UserRoles_GetByUserId`
- `dbo.usp_LoginAttempts_Insert`
- `dbo.usp_AuditLogs_Insert`
- `dbo.usp_Consultants_GetIdByUserId`
- `dbo.usp_Consultants_GetAll`
- `dbo.usp_Consultants_GetById`
- `dbo.usp_Consultants_Create`
- `dbo.usp_Consultants_Update`
- `dbo.usp_Consultants_SetActive`
- `dbo.usp_Managers_GetIdByUserId`
- `dbo.usp_Managers_GetAll`
- `dbo.usp_Managers_GetById`
- `dbo.usp_Managers_Create`
- `dbo.usp_Managers_Update`
- `dbo.usp_Managers_SetActive`
- `dbo.usp_ManagerConsultants_Exists`
- `dbo.usp_ManagerConsultants_Assign`
- `dbo.usp_ManagerConsultants_Remove`
- `dbo.usp_ManagerConsultants_GetByManagerId`
- `dbo.usp_HolidayDefinitions_GetAll`
- `dbo.usp_HolidayDefinitions_GetById`
- `dbo.usp_HolidayDefinitions_Create`
- `dbo.usp_HolidayDefinitions_Update`
- `dbo.usp_HolidayDefinitions_SetActive`
- `dbo.usp_HolidayDefinitions_Delete`
- `dbo.usp_OfficialHolidayDefinitions_GetAll`
- `dbo.usp_OfficialHolidayDefinitions_GetById`
- `dbo.usp_OfficialHolidayDefinitions_Create`
- `dbo.usp_OfficialHolidayDefinitions_Update`
- `dbo.usp_OfficialHolidayDefinitions_Delete`
- `dbo.usp_LeaveRequests_ExistsOverlap`
- `dbo.usp_LeaveRequests_Create`
- `dbo.usp_LeaveRequests_GetMine`
- `dbo.usp_LeaveRequests_GetById`
- `dbo.usp_LeaveRequests_GetPendingForManager`
- `dbo.usp_LeaveRequests_GetPendingForAdmin`
- `dbo.usp_LeaveRequests_GetForReview`
- `dbo.usp_LeaveRequests_GetConflicts`
- `dbo.usp_LeaveRequests_Approve`
- `dbo.usp_LeaveRequests_Reject`
- `dbo.usp_WorkforceTimeline_GetForAdmin`
- `dbo.usp_WorkforceTimeline_GetForManager`
- `dbo.usp_OrganizationCalendar_GetForAdmin`
- `dbo.usp_OrganizationCalendar_GetForManager`
- `dbo.usp_OrganizationCalendar_GetForConsultant`
- `dbo.usp_OrganizationCalendar_GetDetailForAdmin`
- `dbo.usp_OrganizationCalendar_GetDetailForManager`
- `dbo.usp_OrganizationCalendar_GetDetailForConsultant`
- `dbo.usp_Dashboard_GetForAdmin`
- `dbo.usp_Dashboard_GetForManager`
- `dbo.usp_Dashboard_GetForConsultant`
- `dbo.usp_Reports_ConsultantLeaveUsage`
- `dbo.usp_Reports_MonthlyLeaveActivity`
- `dbo.usp_Reports_TeamLeaveUsage`
- `dbo.usp_Reports_PeakLeaveDays`
- `dbo.usp_Reports_StatusDistribution`
- `dbo.usp_Reports_UpcomingLeaves`
- `dbo.usp_Reports_UpcomingHolidays`
- `dbo.usp_Reports_RecentLeaveRequests`
- Development-only bootstrap procedures: `dbo.usp_Users_Upsert`, `dbo.usp_UserRoles_Ensure`, `dbo.usp_Consultants_EnsureForUser`, `dbo.usp_Managers_EnsureForUser`, `dbo.usp_ManagerConsultants_Ensure`

Application code must refer to stored procedure names through centralized Infrastructure constants and must not accept procedure names from user input.

## Seed Data

`db/004_Seed/001_SeedRoles.sql` seeds only generic roles:

- Consultant
- Manager
- Administrator

No real users, real emails, real company data, or production data are seeded.

Development demo users are not stored as plaintext passwords in `/db`. When `LeaveFlow:Development:BootstrapIdentity` is true in Development, the host upserts fictional local users using hashes created at runtime from user secrets or environment variables:

```powershell
$env:LeaveFlow__Development__DemoPassword="..."
```

The local demo users are `admin@leaveflow.local`, `manager@leaveflow.local`, `consultant1@leaveflow.local`, and `consultant2@leaveflow.local`. The two consultants are assigned to the demo manager through development-only stored procedures.

## Configuration

Local development should provide the connection string through user secrets or environment variables:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=LeaveFlow;Integrated Security=true;TrustServerCertificate=true"
```

For Docker SQL Server, copy `.env.example` to `.env`, set a strong `LEAVEFLOW_SQL_PASSWORD`, and use:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=LeaveFlow;User Id=sa;Password=$env:LEAVEFLOW_SQL_PASSWORD;Encrypt=True;TrustServerCertificate=True"
```

`.env` is ignored by git. `.env.example` contains placeholders only and is safe to commit.

White-label branding is intentionally not database-backed in Stage 18. `LeaveFlow:Branding` is read by the Web layer from configuration, so no tables, indexes, stored procedures, seed scripts, or Dapper repositories were added for branding.

The committed `appsettings.json` files contain only the safe connection string name:

```json
{
  "LeaveFlow": {
    "Database": {
      "ConnectionStringName": "DefaultConnection"
    }
  }
}
```

## Transaction Approach

`IDataTransactionFactory` prepares a SQL Server transaction boundary for future multi-step workflows such as leave approval. Stage 2 does not implement business transactions.

Holiday create, update, set-active, and delete repository methods wrap stored procedure calls in an explicit SQL transaction. Create and update regenerate child day rows inside stored procedures; delete removes child rows before the definition row. Future transactional repository methods should continue using stored procedures only.

## Holiday Duplicate And Overlap Policy

Exact duplicate holiday definitions are rejected by name plus inclusive date range. Partial and nested overlaps are allowed because different holiday definitions can legitimately share dates, such as an organization closure overlapping an official public holiday.

## Leave Request Duplicate And Overlap Policy

New leave requests are always created with `Pending` status. For the same consultant, exact duplicates and partial or nested overlaps are rejected when the existing request is `Pending` or `Approved`. `Rejected` requests are ignored by overlap checks so consultants can resubmit corrected ranges.

## Leave Approval And Conflict Policy

Managers can review only leave requests for assigned consultants. Administrators can review all leave requests. Approval and rejection procedures require the request to still be `Pending`. Approval updates the request, writes `ReviewedAt`, `ReviewedBy`, and optional `ReviewNote`, then creates one `ConsultantLeaveDays` row for each inclusive date in the approved range. Rejection updates only the request review fields and does not create leave day rows.

Conflict detection reads approved leave day rows from `ConsultantLeaveDays`, excludes the current request, and returns overlapping dates for the review range. The preview is decision support; conflicts do not automatically block approval.

`ConsultantLeaveDays` has a unique approval-stage index on `ConsultantId`, `LeaveRequestId`, and `LeaveDate` to prevent duplicate day rows for the same approved request.

## Workforce Timeline Policy

The workforce timeline reads from `ConsultantLeaveDays` as the availability source of truth and joins back to `LeaveRequests` with `Status = N'Approved'`. Pending and rejected requests are not visible in the timeline.

Administrators use `dbo.usp_WorkforceTimeline_GetForAdmin` and can optionally filter by manager assignment. Managers use `dbo.usp_WorkforceTimeline_GetForManager`, which scopes rows through `ManagerConsultants` using `@ManagerId`. Consultants do not receive a global/team timeline.

Timeline procedures page consultants server-side, return one or more rows per consultant for the selected date range, and avoid per-consultant loops. Application code builds the day matrix from these rows instead of passing database rows directly to the view.

`db/002_Indexes/006_CreateWorkforceTimelineIndexes.sql` adds `IX_ConsultantLeaveDays_Date_Consultant` on `(LeaveDate, ConsultantId)` including `LeaveRequestId` for date-range timeline reads.

## Organization Calendar Policy

The organization calendar combines three day-level sources: approved consultant leave from `ConsultantLeaveDays`, active organization holidays from `HolidayDays`, and active official holidays from `OfficialHolidayDays`. Pending and rejected leave requests are not calendar inputs.

Administrators use `dbo.usp_OrganizationCalendar_GetForAdmin` and can optionally filter leave events by manager or consultant. Managers use `dbo.usp_OrganizationCalendar_GetForManager`, which scopes leave events through `ManagerConsultants` while still returning organization and official holidays. Consultants use `dbo.usp_OrganizationCalendar_GetForConsultant`, which returns only their own leave events plus holidays and does not return other consultant names.

Calendar detail procedures follow the same role split. Leave details require approved status and role-appropriate consultant scope. Holiday details require active definitions and are visible to authenticated users.

`db/002_Indexes/007_CreateOrganizationCalendarIndexes.sql` adds date-first indexes for `HolidayDays` and `OfficialHolidayDays`. `ConsultantLeaveDays` date-range reads continue to use the Stage 8 date-first index.

## Reporting Policy

Reporting separates approved leave-day analytics from workflow analytics. Approved usage, monthly approved activity, team usage, peak leave days, and upcoming approved leave read from `ConsultantLeaveDays` joined back to approved `LeaveRequests`. Status distribution and recent request snapshots read from `LeaveRequests`. Upcoming holiday reports read active organization and official holiday definitions.

Administrators can run organization-wide reports with optional manager or consultant filters. Managers are always scoped through `ManagerConsultants` using their server-resolved manager id; request parameters cannot expand manager scope. Consultants receive only their personal dashboard and are denied organization-wide report screens.

Reporting ranges default to the current calendar year and are capped at 366 inclusive days in the application layer. `db/002_Indexes/008_CreateReportingIndexes.sql` adds `IX_LeaveRequests_Status_Date_Consultant` for status/date/consultant reporting lookups.

## Security Model

Production database access should use least privilege:

- application user receives execute permission on approved stored procedures
- application user should not receive broad table-level read/write permissions
- production usernames and passwords are managed outside source control
- dynamic SQL is not used
- development bootstrap procedures are not granted to the production application user
