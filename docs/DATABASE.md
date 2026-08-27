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
- holiday days by date
- audit logs by actor, target, and timestamp
- login attempts by normalized email and timestamp
- consultant and manager management lists by active state and name
- holiday management lists by active state, name, and date range
- consultant leave requests by consultant, status, and date range

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
- Development-only bootstrap procedures: `dbo.usp_Users_Upsert`, `dbo.usp_UserRoles_Ensure`, `dbo.usp_Consultants_EnsureForUser`, `dbo.usp_Managers_EnsureForUser`, `dbo.usp_ManagerConsultants_Ensure`

Application code must refer to stored procedure names through centralized Infrastructure constants and must not accept procedure names from user input.

## Seed Data

`db/004_Seed/001_SeedRoles.sql` seeds only generic roles:

- Consultant
- Manager
- Administrator

No real users, real emails, real company data, or production data are seeded.

Development demo users are not stored as plaintext passwords in `/db`. When `LeaveFlow:Development:BootstrapIdentity` is true in Development, the host upserts fictional local users (`consultant@leaveflow.local`, `manager@leaveflow.local`, `administrator@leaveflow.local`) using hashes created at runtime from user secrets or environment variables:

```powershell
$env:LeaveFlow__Development__Passwords__Consultant="..."
$env:LeaveFlow__Development__Passwords__Manager="..."
$env:LeaveFlow__Development__Passwords__Administrator="..."
```

## Configuration

Local development should provide the connection string through user secrets or environment variables:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=LeaveFlow;Integrated Security=true;TrustServerCertificate=true"
```

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

## Security Model

Production database access should use least privilege:

- application user receives execute permission on approved stored procedures
- application user should not receive broad table-level read/write permissions
- production usernames and passwords are managed outside source control
- dynamic SQL is not used
- development bootstrap procedures are not granted to the production application user
