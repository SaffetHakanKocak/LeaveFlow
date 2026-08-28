# Security

## Security Position

Security is a core design requirement for LeaveFlow, not a later enhancement. The system will enforce authentication, authorization, object-level access control, secure persistence, auditability, and safe defaults from the beginning.

## Roles

### Consultant

- Can view own profile.
- Can create own leave requests.
- Can view own leave history.
- Cannot access other consultants' personal calendars or private leave details.

### Manager

- Can view assigned consultants.
- Can review leave requests for assigned consultants.
- Can view leave conflicts and team timeline for their scope.
- Cannot access unrelated teams unless explicitly authorized.

### Administrator

- Can manage users, consultants, managers, holidays, public holidays, and organization reports.
- Can access administrative views and system configuration.
- Administrative actions must be audited.

## Threats To Address

- Broken authentication
- Broken authorization
- IDOR through predictable identifiers
- SQL injection
- Cross-site request forgery
- Cross-site scripting
- Brute force login attempts
- Weak password storage
- Session fixation or insecure cookies
- Secret leakage
- Overly broad database permissions
- Missing audit trails
- Excessive error detail disclosure

## Planned Controls

### Authentication

- LeaveFlow.Web uses ASP.NET Core cookie authentication.
- LeaveFlow.Api uses a deferred authentication scheme so protected endpoints return 401. JWT is not implemented yet.
- Passwords are hashed with ASP.NET Core Identity `PasswordHasher<object>` (PBKDF2 Identity V3). Custom hash algorithms are not used.
- Failed logins increment a stored counter. After `LeaveFlow:Authentication:MaxFailedAccessAttempts` failures (default 5), the account is locked for `LeaveFlow:Authentication:LockoutDurationMinutes` (default 15).
- Login failures return a generic message: `Invalid email or password.`
- No plaintext passwords in logs, database scripts, seed data, or committed configuration.

### Authorization

- Role policies: Consultant, Manager, Administrator.
- Object-level consultant access is evaluated in `IConsultantResourceAuthorizationService`:
  - Administrator: any consultant resource
  - Consultant: only own consultant id
  - Manager: only assigned consultants
- Backend authorization is required. Hiding UI controls is not a security control.
- Administrator-only policies protect consultant and manager management screens and state-changing actions.
- Consultant detail pages reuse object-level authorization so consultants can view only their own profile and managers can view only assigned consultants.
- Administrator-only policies protect organization and official holiday management screens and state-changing actions.
- Consultant leave request screens are consultant-only and resolve the consultant profile from the authenticated user id.
- Leave review screens resolve manager/admin scope server-side and never rely on hidden UI controls for approval authorization.
- Workforce timeline screens deny consultants, scope managers to assigned consultants server-side, and ignore manager filter tampering for manager users.
- Organization calendar responses are role-aware: admins see all approved leave, managers see assigned consultant leave, and consultants see only their own leave plus holidays.

### Data Access

- Dapper only.
- Stored procedures only.
- Parameterized procedure calls.
- Application database user should receive execute permissions for approved procedures.
- No direct table access from application code.

### Web Security

- CSRF protection for MVC form submissions.
- Output encoding in Razor views.
- Security headers.
- Secure, HttpOnly, SameSite=Lax cookies named `.LeaveFlow.Auth`.
- Production cookie `SecurePolicy` is Always. Development/Testing uses SameAsRequest so local HTTP test hosts can authenticate.
- Sliding expiration is enabled. Ticket lifetime is `LeaveFlow:Authentication:Cookie:ExpireTimeSpanMinutes` (default 480). This keeps an active workday session alive without a persistent remember-me flag.
- Model validation at request boundaries.

### API Security

- Consistent authentication challenge behavior.
- Problem Details responses without leaking stack traces.
- Input validation for request models.
- Rate limiting for sensitive endpoints in later phases.

### Audit Logging

Audit logs should capture:

- Actor user id
- Actor role context where relevant
- Action name
- Target resource type and id
- Decision outcome
- Timestamp
- Correlation id
- Request metadata that is safe to store

Sensitive values must be excluded or redacted.

## Security Test Areas

Security tests should cover:

- Consultants cannot access other consultants' leave records.
- Managers cannot approve requests outside their assigned scope.
- Administrators have required administrative access.
- Unauthenticated users cannot access protected endpoints.
- CSRF protection exists for state-changing MVC actions.
- Stored procedure callers use parameters.
- Error responses do not expose internals.

## Stage 1 Security Review

- No secrets, connection strings, API keys, real users, or real company data were added.
- CORS was not enabled because there is no current cross-origin requirement.
- OpenAPI is mapped only in Development.
- API uses centralized exception handling and ProblemDetails.
- Production-style exception responses are tested to avoid leaking exception type, stack details, or sensitive exception messages.
- API and Web apply basic security headers: `X-Content-Type-Options`, `X-Frame-Options`, and `Referrer-Policy`.
- HTTPS redirection is enabled in API and Web hosts.
- Real authentication and authorization are intentionally not implemented until a later explicit stage.

## Stage 2 Security Review

- Dapper and `Microsoft.Data.SqlClient` were added without Entity Framework or DbContext.
- Application and Infrastructure C# code contain no raw SQL command strings.
- Dapper calls use `CommandType.StoredProcedure`.
- Stored procedure names are centralized and not built from user input.
- No real connection string, username, password, API key, real email, real user, or real company data was committed.
- `/db/005_Security` documents least-privilege execute-only database access without hard-coded production credentials.
- Role seed data is generic and safe for an open-source repository.
- Real authentication, password hashing, authorization policies, cookie sessions, lockout, CSRF on auth forms, and login protection were implemented in Stage 3.

## Stage 3 Security Review

- LeaveFlow.Web authenticates with an HttpOnly cookie. LeaveFlow.Api does not issue JWT.
- Passwords are stored only as Identity V3 hashes. Login and logout POSTs require antiforgery tokens.
- LoginAttempts and AuditLogs record success, failure, lockout, and logout without password, cookie, or connection-string values.
- Object-level authorization is enforced in application services, not by hiding buttons.
- Production cookie flags and generic login errors are covered by security tests.
- Development identity bootstrap hashes passwords at runtime and does not commit credentials.

## Stage 4 Security Review

- Consultant and manager management is administrator-only for list, create, edit, and active-state changes.
- Consultant details enforce object-level access: administrators see all, consultants see only self, and managers see only assigned consultants.
- MVC management POST actions use antiforgery validation and bind explicit input models to reduce overposting risk.
- People-management data access continues to use Dapper stored procedure calls only.
- Least-privilege database guidance now includes the approved Stage 4 management procedures.

## Stage 5 Security Review

- Holiday management screens are administrator-only.
- Consultant and manager users are denied from holiday management endpoints.
- Holiday create, update, set-active, and delete POST actions require antiforgery tokens.
- Holiday input uses explicit view models and application input models to reduce overposting risk.
- Holiday data access continues to use Dapper stored procedure calls only, with explicit transactions around write operations.
- Least-privilege database guidance now includes the approved Stage 5 holiday procedures.

## Stage 6 Security Review

- Consultant leave request create/list/detail screens require the Consultant role.
- `ConsultantId`, `Status`, `CreatedAt`, `ReviewedAt`, and `ReviewedBy` are not accepted from form models.
- The application service resolves the consultant profile from the authenticated user id and rejects inactive profiles.
- Detail and list stored procedures are scoped by consultant id to reduce IDOR risk.
- Leave request create POST actions require antiforgery tokens.
- Leave request create writes only to `LeaveRequests`; `ConsultantLeaveDays` remains untouched until approval/conflict stages.
- Least-privilege database guidance now includes the approved Stage 6 leave request procedures.

## Stage 7 Security Review

- Consultants are denied from approval and rejection endpoints.
- Managers can review only leave requests for consultants assigned through `ManagerConsultants`.
- Administrators can review all leave requests.
- Review detail, conflict, approve, and reject procedures enforce reviewer scope using manager assignment or administrator flag.
- Approve/reject POST actions require antiforgery tokens and use explicit review input models.
- Approve/reject procedures require `Pending` status and fail safely for repeated or stale decisions.
- Approval generates `ConsultantLeaveDays` in the same database transaction and a unique index prevents duplicate day rows.

## Stage 8 Security Review

- Consultants are denied from the workforce timeline.
- Managers can view only assigned consultants in the timeline, resolved from their authenticated user id.
- Administrator timeline access can view the organization and optionally filter by manager.
- Manager-supplied `managerId` querystring values are ignored by the application service; manager scope is resolved server-side.
- Timeline stored procedures read only approved `ConsultantLeaveDays` and do not expose pending or rejected requests.
- Timeline view models expose only consultant name/email, active state, leave date, leave request id, and safe reason text.
- Security tests cover anonymous denial, consultant denial, manager scoping, admin access, and manager filter tampering.

## Stage 9 Security Review

- Organization calendar requires authentication.
- Consultants receive only their own approved leave plus organization/official holidays; other consultant names and leave details are not returned.
- Managers receive only assigned consultant approved leave plus organization/official holidays.
- Administrators can view organization-wide calendar data and optionally filter by manager or consultant.
- Manager and consultant querystring tampering is ignored or scoped server-side by the application service and stored procedures.
- Calendar detail access is role-aware and prevents cross-consultant and cross-team leave IDOR.
- Calendar list/detail data excludes pending and rejected leave requests and inactive holiday definitions.

## Open Security Decisions

- API token/JWT design remains deferred until an API client stage.
- Password complexity policy beyond length/required fields.
- Rate limit thresholds beyond account lockout.
- Audit log retention policy.
