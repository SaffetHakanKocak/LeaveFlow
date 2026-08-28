using LeaveFlow.Application.Abstractions.Calendar;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.People;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.Application.Calendar;

public sealed class OrganizationCalendarService(
    IOrganizationCalendarRepository repository,
    IUserRoleRepository userRoleRepository,
    IConsultantIdentityRepository consultantIdentityRepository,
    IManagerIdentityRepository managerIdentityRepository,
    TimeProvider timeProvider) : IOrganizationCalendarService
{
    public async Task<OrganizationCalendarResult?> GetAsync(
        Guid actorUserId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(query);
        if (!Validate(normalized).IsValid)
        {
            return null;
        }

        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(actorUserId, cancellationToken);
        var rows = await GetRowsAsync(actorUserId, roles, normalized, cancellationToken);
        if (rows is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var days = OrganizationCalendarDateRange.GenerateDays(
            normalized.StartDate!.Value,
            normalized.EndDate!.Value,
            today,
            normalized.ViewMode ?? OrganizationCalendarDateRange.MonthView);

        return new OrganizationCalendarResult(
            normalized,
            days,
            CalendarEventNormalizer.Normalize(rows),
            OrganizationCalendarSettings.MaxRangeDays);
    }

    public async Task<CalendarEventDetail?> GetDetailAsync(
        Guid actorUserId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!CalendarEventTypes.All.Contains(query.EventType))
        {
            return null;
        }

        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(actorUserId, cancellationToken);
        if (roles.Contains(RoleNames.Administrator, StringComparer.Ordinal))
        {
            return await repository.GetDetailForAdminAsync(query, cancellationToken);
        }

        if (roles.Contains(RoleNames.Manager, StringComparer.Ordinal))
        {
            var managerId = await managerIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            return managerId is null
                ? null
                : await repository.GetDetailForManagerAsync(managerId.Value, query, cancellationToken);
        }

        if (roles.Contains(RoleNames.Consultant, StringComparer.Ordinal))
        {
            var consultantId = await consultantIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            return consultantId is null
                ? null
                : await repository.GetDetailForConsultantAsync(consultantId.Value, query, cancellationToken);
        }

        return null;
    }

    public ValidationResult Validate(OrganizationCalendarQuery query)
    {
        return OrganizationCalendarValidation.Validate(Normalize(query));
    }

    private async Task<IReadOnlyList<OrganizationCalendarDataRow>?> GetRowsAsync(
        Guid actorUserId,
        IReadOnlyList<string> roles,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken)
    {
        if (roles.Contains(RoleNames.Administrator, StringComparer.Ordinal))
        {
            return await repository.GetForAdminAsync(query, cancellationToken);
        }

        if (roles.Contains(RoleNames.Manager, StringComparer.Ordinal))
        {
            var managerId = await managerIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            return managerId is null
                ? null
                : await repository.GetForManagerAsync(managerId.Value, query with { ManagerId = null }, cancellationToken);
        }

        if (roles.Contains(RoleNames.Consultant, StringComparer.Ordinal))
        {
            var consultantId = await consultantIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            return consultantId is null
                ? null
                : await repository.GetForConsultantAsync(consultantId.Value, query with { ManagerId = null, ConsultantId = null }, cancellationToken);
        }

        return null;
    }

    private OrganizationCalendarQuery Normalize(OrganizationCalendarQuery query)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var viewMode = string.Equals(query.ViewMode, OrganizationCalendarDateRange.WeekView, StringComparison.OrdinalIgnoreCase)
            ? OrganizationCalendarDateRange.WeekView
            : OrganizationCalendarDateRange.MonthView;

        var startDate = query.StartDate ?? (viewMode == OrganizationCalendarDateRange.WeekView
            ? OrganizationCalendarDateRange.StartOfWeek(today)
            : new DateOnly(today.Year, today.Month, 1));
        var endDate = query.EndDate ?? (viewMode == OrganizationCalendarDateRange.WeekView
            ? OrganizationCalendarDateRange.EndOfWeek(startDate)
            : startDate.AddMonths(1).AddDays(-1));

        return query with
        {
            StartDate = startDate,
            EndDate = endDate,
            ViewMode = viewMode
        };
    }
}
