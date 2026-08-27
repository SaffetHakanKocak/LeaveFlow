# Database

## Database Principles

LeaveFlow uses Microsoft SQL Server as the primary relational database.

Application code must access the database only through stored procedures using Dapper. Raw SQL statements in application code are not allowed.

SQL objects are kept in the repository under `/db`.

## Proposed SQL Folder Structure

```text
db/
  001_Tables/
  002_Indexes/
  003_StoredProcedures/
  004_Seed/
  005_Security/
  006_TestData/
```

Scripts should be ordered and either repeatable or accompanied by clear installation instructions.

## Initial Entity/Table List

### Identity and Access

- Users
- Roles
- UserRoles
- UserLoginAttempts
- UserSessions
- PasswordResetTokens

### People and Organization

- Employees
- Consultants
- Managers
- Departments
- Teams
- TeamMembers
- ManagerAssignments

### Leave Management

- LeaveTypes
- LeaveBalances
- LeaveRequests
- LeaveRequestDays
- LeaveRequestStatusHistory
- LeavePolicies
- LeavePolicyRules

### Approval Workflow

- ApprovalSteps
- ApprovalAssignments
- ApprovalDecisions

### Calendar and Holidays

- OrganizationCalendars
- CalendarEvents
- PublicHolidays
- CompanyHolidays
- WorkingDayRules
- RegionalCalendars

### Availability and Timeline

- AvailabilitySnapshots
- TeamAvailabilitySummaries
- LeaveConflictChecks

### Reporting

- ReportDefinitions
- ReportRuns
- ReportExports

### Audit and Security

- AuditLogs
- SecurityEvents
- DataAccessLogs

### System Configuration

- SystemSettings
- FeatureFlags
- WhiteLabelSettings

## Core Relationships

- A user can have one or more roles.
- An employee profile belongs to a user account.
- Consultants and managers are employee role specializations from the business perspective.
- A manager can be assigned to many consultants through manager assignments or team membership.
- A leave request belongs to one consultant.
- A leave request has one leave type and status history.
- Approval decisions are linked to leave requests and approvers.
- Holidays and calendar events belong to organization or regional calendars.
- Audit logs reference the actor, action, target resource, and request metadata.

## Stored Procedure Naming

Stored procedures should use a consistent naming pattern:

```text
dbo.usp_<Module>_<Action>
```

Examples:

```text
dbo.usp_LeaveRequests_Create
dbo.usp_LeaveRequests_GetByIdForActor
dbo.usp_LeaveRequests_GetForConsultant
dbo.usp_LeaveRequests_GetForManagerTeam
dbo.usp_LeaveRequests_Approve
dbo.usp_LeaveRequests_Reject
dbo.usp_PublicHolidays_GetByCalendar
dbo.usp_AuditLogs_Create
```

## Security Rules For Data Access

- Stored procedures must support object-level authorization where practical.
- Sensitive read procedures should accept the actor user id and role context where needed.
- Procedures must not expose data outside the caller's authorization scope.
- Application code must use parameterized stored procedure calls.
- Database permissions should allow the application account to execute approved procedures, not directly read/write all tables.

## Initial Indexing Considerations

Likely indexes:

- Users by email or normalized username
- UserRoles by user id and role id
- Employees by user id
- ManagerAssignments by manager id and consultant id
- LeaveRequests by consultant id, status, start date, and end date
- LeaveRequestStatusHistory by leave request id and changed date
- PublicHolidays by calendar id and date
- AuditLogs by actor id, target resource, and created date

Final indexes should be based on actual query patterns from stored procedure design.
