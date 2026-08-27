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

No migration framework is used in Stage 2.

## Tables

- `Roles`: system roles such as Consultant, Manager, and Administrator.
- `Users`: identity account profile fields that are safe before authentication implementation.
- `UserRoles`: many-to-many assignment between users and roles.
- `Consultants`: consultant profile root linked to a user.
- `Managers`: manager profile root linked to a user.
- `ManagerConsultants`: manager-to-consultant assignment scope.
- `LeaveRequests`: leave request header data for later workflow implementation.
- `ConsultantLeaveDays`: per-day leave expansion for availability and conflict checks.
- `HolidayDefinitions`: organization holiday grouping.
- `HolidayDays`: organization holiday dates.
- `OfficialHolidayDefinitions`: official holiday grouping by country and optional region.
- `OfficialHolidayDays`: official holiday dates.
- `AuditLogs`: security and business audit trail.
- `LoginAttempts`: future authentication throttling and audit support.

`RefreshTokens` was intentionally not added in Stage 2 because the authentication strategy is not decided yet.

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

Indexes should be revisited when real stored procedure query patterns are implemented.

## Stored Procedures

Naming convention:

```text
dbo.usp_<Module>_<Action>
```

Current stored procedures:

- `dbo.usp_Roles_GetAll`
- `dbo.usp_Users_GetById`

Application code must refer to stored procedure names through centralized Infrastructure constants and must not accept procedure names from user input.

## Seed Data

`db/004_Seed/001_SeedRoles.sql` seeds only generic roles:

- Consultant
- Manager
- Administrator

No real users, real emails, real company data, or production data are seeded.

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

Future transactional repository methods should receive or compose transaction-aware execution explicitly and continue using stored procedures only.

## Security Model

Production database access should use least privilege:

- application user receives execute permission on approved stored procedures
- application user should not receive broad table-level read/write permissions
- production usernames and passwords are managed outside source control
- dynamic SQL is not used in Stage 2
