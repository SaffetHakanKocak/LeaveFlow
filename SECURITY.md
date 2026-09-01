# Security Policy

## Supported Versions

LeaveFlow is currently a v1.0 release candidate portfolio project. Security fixes are accepted for the main branch.

## Reporting A Vulnerability

Please do not open a public issue for suspected vulnerabilities. Report privately to the repository owner or maintainer with:

- affected area
- reproduction steps
- expected impact
- suggested fix, if known

Do not include real credentials, production data, or private screenshots in reports.

## Security Model Summary

- ASP.NET Core MVC cookie authentication for the web app.
- Role-based access control for Consultant, Manager, and Administrator.
- Backend object-level authorization for consultant data and manager assignment scope.
- CSRF protection for MVC forms.
- Secure cookie defaults and security headers.
- Dapper stored-procedure-only database access.
- No Entity Framework, `DbContext`, raw SQL in C# source, or `CommandType.Text`.
- Optional Azure AI integration is disabled by default.
- AI tool calling is read-only, allowlisted, role-scoped, audited, and has no direct database access.

See `docs/SECURITY.md` for the full security architecture and threat matrix.
