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

Status: next.

Deliverables:

- Leave type management.
- Leave balance model.
- Manager approval and rejection.
- Status history and audit logging.
- Conflict checks.

## Phase 8 - Calendar and Timeline

Deliverables:

- Organization calendar.
- Public holiday management.
- Company holiday management.
- Team timeline view.
- Availability summaries.

## Phase 9 - Reporting

Deliverables:

- Administrator reports.
- Manager team reports.
- Leave usage summaries.
- Export foundation.
- Report run auditing.

## Phase 10 - Hardening and Release Preparation

Deliverables:

- Security review.
- Integration test coverage.
- Security test coverage.
- CI workflow.
- Documentation cleanup.
- Open-source repository polish.
- License and contribution guidance.

## Future Enhancements

- White-label customization.
- Multi-organization support.
- Calendar integration.
- Notification system.
- Advanced policy engine.
- Localization.
