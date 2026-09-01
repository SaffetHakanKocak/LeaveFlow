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
- Reporting dashboards are role-aware: administrators see organization metrics, managers see assigned-team metrics, and consultants see only personal summary data.
- Report screens are administrator/manager only; manager report scope is resolved server-side and consultant users are denied organization-wide analytics.

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
- Authenticated navigation is role-aware for usability, while backend authorization remains the source of truth.
- Theme preference is stored client-side only and does not contain sensitive information.

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

## Stage 10 Security Review

- Dashboard and report endpoints require authentication.
- Dashboards return only role-appropriate data: administrators receive organization metrics, managers receive assigned-team metrics, and consultants receive only personal leave/request data plus holidays.
- Consultant users are denied from organization-wide report screens.
- Manager report scope is resolved from the authenticated user id; supplied `managerId` and `consultantId` values cannot expand visibility.
- Reporting stored procedures keep manager scoping tied to `ManagerConsultants`.
- Reporting data access remains Dapper stored-procedure-only with parameterized calls and centralized procedure names.
- Security tests cover anonymous denial, consultant report denial, manager scoping, manager filter tampering, admin visibility, and consultant dashboard privacy.

## Stage 11 Security Review

- UI refactor did not change controller authorization attributes, application services, stored procedure callers, or object-level authorization rules.
- Logout remains a POST form and keeps antiforgery protection.
- Critical MVC forms keep antiforgery tokens after the layout/design refresh.
- Role-aware sidebar links are convenience only; unauthorized protected routes remain denied by backend policies.
- User-generated leave reason and review note data remains Razor-encoded; the Reports view no longer uses raw `HtmlString` rendering.
- Security regression tests cover login rendering, shell/theme toggle presence, role-specific navigation, antiforgery token presence, and denied unauthorized routes.

## Stage 12 Security Hardening Review

- Login success explicitly signs out any existing web cookie before issuing a new authentication ticket to reduce session fixation risk.
- Web responses include centralized security headers: `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `X-Permitted-Cross-Domain-Policies`, and `Permissions-Policy`.
- CSP limits default loading to same-origin, denies object embedding, denies framing through `frame-ancestors`, and restricts form posts to same-origin.
- Open redirect protection remains based on `Url.IsLocalUrl`; tests cover malicious external `returnUrl` values.
- Logout remains POST-only and antiforgery-protected.
- XSS regressions cover user-controlled leave reasons, review notes, and holiday names.
- Role tampering regressions cover a consultant attempting an administrator-only POST action with overposted role data.
- Cross-user data leakage tests continue to cover dashboard, reports, timeline, and calendar manager/consultant scopes.

## Stage 13 AI Security Review

- AI is disabled by default and LeaveFlow remains fully functional without AI configuration.
- The AI assistant is authenticated, CSRF-protected, and exposed as a simple chat UI only.
- Application exposes provider-neutral `IAiChatClient` and `IAiAssistantService` abstractions; Infrastructure supplies the Azure implementation.
- AI has no direct database access, no repository dependencies, no stored procedure execution path, and no SQL/query execution tool.
- User prompts are length-limited and validated before provider calls.
- Prompt text, credentials, API keys, tokens, and provider response bodies are not logged.
- Provider failures, invalid configuration, and timeouts return safe user-facing errors instead of crashing the host.
- Azure credentials must be supplied through environment variables or user secrets, for example `AI__Azure__ApiKey`; committed config contains only non-secret defaults.
- Security tests cover disabled AI behavior, unauthorized access, antiforgery on AI POST, provider unavailability, invalid provider config, prompt validation, and AI secret leakage regression.

## Stage 14 AI Tool Calling Security Review

- AI tool calling is allowlist-based through `IAiToolRegistry`; there are no SQL/query/database execution tools.
- Registered tools call existing Application services only, preserving the Application -> Dapper -> Stored Procedure data path.
- Tool execution uses the authenticated `UserId`; model-provided role, consultant, and manager identifiers cannot expand scope.
- Consultant scope remains personal, manager scope remains assigned-team, and administrator scope remains organization-wide through existing services.
- Tool arguments are parsed as JSON and validated for type, required IDs, date ranges, and allowed status values before execution.
- Tool call audit records store tool name, user id, timestamp, success/failure, duration, and error code only. Prompt text, tool arguments, result content, credentials, and tokens are not logged.
- Prompt injection attempts such as role claims or instruction overrides remain subject to backend authorization and tool scoping.
- Tests cover prompt injection, role tampering, all-consultant data requests, cross-user data requests, invalid arguments, unknown tools, tool failures, HTTP auth/CSRF, and secret leakage.

## Stage 15 Intelligent Workforce Query Security Review

- Natural-language workforce query planning is deterministic and runs in Application before provider fallback.
- Supported prompts map only to the existing read-only tool allowlist; no write workflow, SQL, repository, stored procedure, or generic database query tool was added.
- Counts, dates, names, statuses, availability, conflicts, and holiday facts are formatted only from tool results.
- Consultant, manager, and administrator scope remains enforced by the existing services behind each tool.
- Ambiguous date or scope requests ask for clarification. Empty results produce no-data responses instead of invented records.
- Domain-external prompts receive a LeaveFlow-only response, limiting the assistant from becoming a general-purpose chatbot.
- Prompt injection cannot change the allowlist, role scope, authenticated user id, or tool arguments trusted by backend authorization.
- Tests cover intent routing, relative date handling, no-data, multi-tool flow, role scope, hallucination/domain guards, prompt injection, and provider/tool failure.

## Stage 16 Quality Gate Security Review

- Authentication, authorization, consultant/manager management, manager assignment, holidays, leave requests, approve/reject, conflict detection, generated leave days, timeline, calendar, dashboard/reports, and AI security tests were reviewed and re-run.
- Source guard searches found no production Entity Framework, `DbContext`, `CommandType.Text`, raw SQL execution, hard-coded secrets, sensitive logging, swallowed exceptions, or sync-over-async issues.
- A sync-over-async call in security test setup was removed to keep the regression harness clean.
- Regression coverage was added for AI leap-year relative date handling and invalid approve transition failure behavior.
- Real local SQL smoke could not be completed because no usable SQL Server/Docker SQL instance or credentials were available in the current environment.

## Stage 17 Public Repository Security Review

- Docker Compose starts SQL Server with `LEAVEFLOW_SQL_PASSWORD` from local environment or `.env`; no SA password is committed.
- `.env.example` contains placeholders only. Real `.env` files, secrets, logs, coverage output, publish output, and local database artifacts are ignored by git.
- Development demo users are bootstrapped only in the Development environment and only when a demo password is configured.
- Azure AI stays disabled by default. The application continues to run without `AI__Azure__ApiKey`.
- Public-repo audit found no real company names, proprietary data, real person data, committed API keys/tokens/passwords, private connection strings, internal endpoints, local absolute user paths, or private assets/logos.
- Stage 16 real database smoke remains a manual release check and is not marked completed by Stage 17 documentation.

## Stage 18 White-Label Security Review

- Branding configuration is presentation-only in LeaveFlow.Web and cannot change authorization policies, role scope, routes, database access, stored procedures, or AI tool allowlists.
- Brand text is rendered through Razor encoding.
- `PrimaryBrandColor` is ignored unless it is a hex color in `#RGB` or `#RRGGBB` format.
- `LogoUrl` is ignored unless it is an app-local `/...` path or an absolute `http`/`https` URL.
- `SupportEmail` is ignored unless it validates as an email address.
- Regression tests cover default branding, custom branding, missing logo fallback, invalid color fallback, dark-mode shell behavior, and HTML/CSS/JavaScript injection attempts through branding config.

## Stage 19 Final Security And Public Repository Review

- Authentication, authorization, IDOR, CSRF, XSS, security headers, secure cookies, lockout, open redirect, secret leakage, AI prompt injection, AI role scope, and AI direct database access prohibitions were reviewed against existing tests and source guards.
- Production source remains free of Entity Framework, `DbContext`, raw SQL execution, `CommandType.Text`, hard-coded secrets, unsafe logging findings, swallowed empty catches, and sync-over-async findings.
- Public-repo audit found no restricted company references, proprietary content, real person data, private endpoints, committed secrets/API keys/passwords, private connection strings, private logos/assets, or local absolute user paths.
- Repository-level `SECURITY.md` now documents private vulnerability reporting and summarizes the security model.

## Threat Matrix

| Threat | Primary risk | Current controls | Regression coverage |
| --- | --- | --- | --- |
| Broken authentication | Unauthorized account access | PBKDF2 password hashing, generic login errors, inactive user denial, lockout, login audit records | Login success/failure, inactive user, locked user, failed count reset |
| Session fixation | Reuse of a pre-authentication ticket | Existing cookie is signed out before a successful sign-in, server-generated auth ticket | Login cookie and authentication flow tests |
| Insecure cookies | Cookie theft or weak transport defaults | HttpOnly, SameSite=Lax, production SecurePolicy=Always, explicit cookie name and lifetime | Authentication cookie configuration tests |
| Broken authorization | Vertical privilege escalation | Role policies on controllers/actions, backend authorization as source of truth | Admin/manager/consultant route access tests |
| IDOR | Horizontal access to another user's resources | Object-level consultant, manager assignment, leave detail, calendar detail, and report scoping | Consultant/manager cross-resource denial tests |
| Query tampering | Expanded data scope through querystring filters | Manager/consultant scopes resolved server-side, tampered filters ignored or constrained | Timeline, calendar, and reports tampering tests |
| CSRF | Forged state-changing form posts | Global MVC antiforgery validation plus explicit POST attributes on critical actions | Login, logout, leave, review, people, and holiday POST tests |
| XSS | Script execution through user-controlled text | Razor output encoding, no `Html.Raw` in views, CSP defense-in-depth | Reason, ReviewNote, holiday Name encoding tests and source guard |
| Overposting | Client sets server-owned fields | Explicit form view models, service-side actor resolution, ignored posted security fields | Leave request, review, holiday overposting tests |
| SQL injection | User input changes SQL commands | Dapper stored procedures only, no EF/DbContext, no raw SQL in application/infrastructure | Data access and stored procedure contract tests |
| Sensitive error leakage | Stack traces or internal details exposed | Production exception handler and ProblemDetails behavior | Production error response tests |
| Secret leakage | Passwords/connection strings in repo or logs | Demo password from environment/user secrets, source and SQL secret scans, logging by user id | Secret leakage guard tests |
| AI tool abuse | Model attempts to expand scope or call unsafe tools | Deterministic query planning for supported workforce questions, allowlisted tools, authenticated user context, service-level authorization, no SQL/query tools | AI natural-language and tool calling unit tests, HTTP security tests |
| Clickjacking | App framed by attacker | `X-Frame-Options: DENY` and CSP `frame-ancestors 'none'` | Security header tests |
| MIME sniffing | Browser interprets content unsafely | `X-Content-Type-Options: nosniff` | Security header tests |
| Concurrency regression | Duplicate approval day rows or stale decisions | Pending-only approval, unique day-row behavior, repository transaction/concurrency guards | Repeated approval concurrency test |

## Open Security Decisions

- API token/JWT design remains deferred until an API client stage.
- Password complexity policy beyond length/required fields.
- Rate limit thresholds beyond account lockout.
- Audit log retention policy.
- CSP currently allows inline scripts/styles to remain compatible with the existing Razor layout, Bootstrap validation, and inline confirmation handlers. A future stricter CSP should move inline scripts to external files or nonced scripts.
- Least-privilege database execution permissions are documented; enforcement depends on the production SQL login/user provisioned outside the application.
