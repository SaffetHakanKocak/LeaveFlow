# Roadmap

## Phase 0 - Planning

Status: completed for the initial scope.

Deliverables:

- Technical plan
- Solution architecture
- Folder structure plan
- Domain module list
- Initial database entity/table list
- Security model summary
- Development roadmap
- Documentation context system

## Phase 1 - Solution Skeleton

Status: completed.

Deliverables:

- Create .NET solution.
- Create `src` projects.
- Create `tests` projects.
- Configure project references according to architecture.
- Add baseline dependency injection structure.
- Add baseline configuration files without secrets.
- Add initial README.
- Add formatting and editor configuration.
- Run restore, build, and test commands.

No business workflows should be implemented in this phase unless explicitly requested.

## Phase 2 - Database Foundation

Status: completed.

Deliverables:

- Create `/db` structure.
- Add initial table scripts.
- Add indexes.
- Add stored procedure conventions.
- Add seed data that uses only fictional generic values.
- Add database setup documentation.
- Add database security script for least-privilege execution model.

## Phase 3 - Identity and Access Foundation

Status: completed.

Deliverables:

- Implement authentication baseline.
- Implement roles: Consultant, Manager, Administrator.
- Implement password hashing and login protection.
- Implement backend authorization policies.
- Add audit logging foundation.
- Add security tests for protected resources.

## Phase 4 - People and Organization

Status: completed.

Deliverables:

- User and employee management.
- Consultant and manager profiles.
- Manager assignment model.
- Administrator management endpoints and screens.
- Object-level authorization tests.

## Phase 5 - Holiday Management

Status: completed.

Deliverables:

- Organization holiday management.
- Official holiday management.
- Inclusive day-row generation.
- Transactional create, update, and delete behavior.
- Duplicate holiday protection.

## Phase 6 - Consultant Leave Request

Status: completed.

Deliverables:

- Consultant leave request creation.
- Leave request history.
- Draft and submit behavior.
- Request validation.
- Consultant scoped access.

## Phase 7 - Leave Approval & Conflict Detection

Status: completed.

Deliverables:

- Leave type management.
- Leave balance model.
- Manager approval and rejection.
- Status history and audit logging.
- Conflict checks.

## Phase 8 - Workforce Leave Timeline

Status: completed.

Deliverables:

- Team timeline view.
- Approved leave day visualization.
- Admin organization-wide access with optional manager filter.
- Manager assigned-consultant scope.
- Date range, consultant search, inactive consultant filter, and server-side pagination.
- Timeline performance index and stored procedure contracts.

## Phase 9 - Organization Calendar

Status: completed.

Deliverables:

- Organization calendar view.
- Combined official and organization holiday visibility.
- Approved leave visibility in calendar context.
- Role-appropriate calendar scoping.

## Phase 10 - Reporting

Status: completed.

Deliverables:

- Administrator reports.
- Manager team reports.
- Leave usage summaries.
- Role-aware dashboards for administrators, managers, and consultants.
- Report range filters, summary cards, and report tables.
- Reporting authorization and manager scope tampering tests.

Export foundation and report run auditing remain future enhancements.

## Phase 11 - Professional UI/UX Pass

Status: completed.

Deliverables:

- Improve visual consistency across existing MVC screens.
- Refine dashboard/report readability and responsive behavior.
- Polish navigation, spacing, empty states, forms, and tables without changing business scope.
- Add responsive sidebar/topbar application shell.
- Add light/dark theme support.
- Add UI/security regression coverage for shell, role navigation, and antiforgery preservation.

## Phase 12 - Security Hardening

Status: completed.

Deliverables:

- Focused security review.
- Authorization and CSRF hardening.
- Error handling, headers, and cookie review.
- Dependency and secret scanning readiness.

## Phase 13 - Azure AI Assistant Foundation

Status: completed.

Deliverables:

- Optional AI feature flag and configuration.
- Provider-neutral Application AI abstractions.
- Azure AI provider in Infrastructure.
- Authenticated MVC `AI Asistan` screen.
- Prompt validation, timeout handling, safe provider errors, and secret-safe logging.
- Tests and documentation for AI foundation security boundaries.

## Phase 14 - Secure AI Tool Calling

Status: completed.

Deliverables:

- Design and implement constrained AI tool calling through existing application services only.
- Enforce authorization, audit, and allowlists for any AI-assisted business action.
- Continue prohibiting direct SQL/database access from AI.

## Phase 15 - Intelligent Workforce Queries

Status: completed.

Deliverables:

- Add richer natural-language workforce questions over the approved AI tool boundary.
- Improve query interpretation without bypassing authorization or stored-procedure data access.
- Keep AI actions read-only unless a later stage explicitly approves write workflows.

Completed notes:

- Supported deterministic Turkish workforce intents for upcoming leaves, team availability, date-range conflict checks, peak leave days, request status counts, upcoming holidays, personal leave usage, and team leave summaries.
- Relative date handling covers today, tomorrow, this week, next week, this month, next month, and this year.
- Multi-tool answers use only existing read-only tools and keep role scope server-side.
- Domain, hallucination, no-data, prompt-injection, and provider/tool failure guards are covered by tests.

## Phase 16 - Hardening and Release Preparation

Status: completed for the Stage 16 quality gate scope.

Deliverables:

- Security review.
- Integration test coverage.
- Security test coverage.
- Documentation cleanup.

Completed notes:

- Re-ran clean, restore, build, and test quality gates with 0 build warnings, 0 build errors, and 0 failed tests.
- Reviewed guards for forbidden data access patterns, hard-coded secrets, sensitive logging, swallowed exceptions, and sync-over-async.
- Added targeted regression coverage for AI leap-year date handling and invalid approve transition failure behavior.
- Local SQL smoke was attempted but blocked by unavailable local SQL/Docker SQL configuration.

Deferred to Stage 17:

- Docker setup.
- CI workflow.
- Open-source repository polish.
- License and contribution guidance.

## Future Enhancements

- White-label customization.
- Multi-organization support.
- Calendar integration.
- Notification system.
- Advanced policy engine.
- Localization.
