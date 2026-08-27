using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.People;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.Application.Timeline;

public sealed class WorkforceTimelineService(
    IWorkforceTimelineRepository repository,
    IUserRoleRepository userRoleRepository,
    IManagerIdentityRepository managerIdentityRepository,
    TimeProvider timeProvider) : IWorkforceTimelineService
{
    public async Task<WorkforceTimelineResult?> GetAsync(
        Guid actorUserId,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(query);
        if (!Validate(normalized).IsValid)
        {
            return null;
        }

        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(actorUserId, cancellationToken);
        var rows = roles.Contains(RoleNames.Administrator, StringComparer.Ordinal)
            ? await repository.GetForAdminAsync(normalized, cancellationToken)
            : await GetManagerRowsAsync(actorUserId, roles, normalized, cancellationToken);

        if (rows is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var days = TimelineDateRange.GenerateDays(normalized.StartDate!.Value, normalized.EndDate!.Value, today);
        var timelineRows = WorkforceTimelineMatrixBuilder.Build(rows.Items, days);

        return new WorkforceTimelineResult(
            normalized,
            days,
            timelineRows,
            rows.TotalCount,
            WorkforceTimelineSettings.MaxRangeDays);
    }

    public ValidationResult Validate(WorkforceTimelineQuery query)
    {
        return WorkforceTimelineValidation.Validate(Normalize(query));
    }

    private async Task<LeaveFlow.Application.Common.PagedResult<WorkforceTimelineDataRow>?> GetManagerRowsAsync(
        Guid actorUserId,
        IReadOnlyList<string> roles,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken)
    {
        if (!roles.Contains(RoleNames.Manager, StringComparer.Ordinal))
        {
            return null;
        }

        var managerId = await managerIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
        return managerId is null
            ? null
            : await repository.GetForManagerAsync(managerId.Value, query with { ManagerId = null }, cancellationToken);
    }

    private WorkforceTimelineQuery Normalize(WorkforceTimelineQuery query)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var startDate = query.StartDate ?? new DateOnly(today.Year, today.Month, 1);
        var endDate = query.EndDate ?? startDate.AddMonths(1).AddDays(-1);

        return query with
        {
            StartDate = startDate,
            EndDate = endDate,
            ConsultantSearch = string.IsNullOrWhiteSpace(query.ConsultantSearch) ? null : query.ConsultantSearch.Trim(),
            PageNumber = Math.Max(1, query.PageNumber),
            PageSize = Math.Clamp(query.PageSize, 1, 100)
        };
    }
}
