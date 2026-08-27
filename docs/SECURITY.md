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

- Secure password hashing with a modern password hasher.
- Lockout or throttling for repeated failed login attempts.
- Secure session handling.
- No plaintext passwords in logs, database scripts, seed data, or configuration.

### Authorization

- Policy-based authorization in API and Web layers.
- Object-level authorization in application services and stored procedures where appropriate.
- Backend authorization for every sensitive resource.
- UI visibility is treated only as usability, not security.

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
- Secure, HttpOnly, SameSite cookies.
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
- Real authentication, password hashing, authorization policies, token/session design, and login protection remain intentionally deferred to Stage 3.

## Open Security Decisions

- Exact authentication mechanism for the first implementation phase.
- Session-based web auth versus token-based API auth strategy.
- Password policy details.
- Rate limit thresholds.
- Audit log retention policy.
