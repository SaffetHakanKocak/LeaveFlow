# v1.0.0 Release Checklist

## Automated Quality Gate

- [ ] `dotnet clean`
- [ ] `dotnet restore`
- [ ] `dotnet build --no-restore`
- [ ] `dotnet test --no-build`
- [ ] Confirm build output has 0 warnings and 0 errors.
- [ ] Confirm all automated tests pass.

## Manual Real Database Smoke Test

Run against Docker SQL Server and a local `LeaveFlow.Web` instance with development demo accounts:

- [ ] admin login
- [ ] admin dashboard
- [ ] consultant leave request creation
- [ ] manager approval
- [ ] conflict detection
- [ ] `ConsultantLeaveDays` inclusive day generation
- [ ] workforce timeline
- [ ] organization calendar
- [ ] reports
- [ ] authorization negative checks
- [ ] AI disabled/unavailable state

## Public Repository Review

- [ ] No company-specific or proprietary content.
- [ ] No real person/user data.
- [ ] No private endpoints.
- [ ] No committed secrets, API keys, passwords, or private connection strings.
- [ ] No private logos or assets.
- [ ] No local absolute user paths.
- [ ] `.env` remains ignored and `.env.example` contains placeholders only.

## Release Commands

Do not run these until the checklist is complete and the release commit is approved:

```powershell
git tag v1.0.0
git push origin v1.0.0
```
