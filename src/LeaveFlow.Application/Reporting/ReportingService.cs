using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.People;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.Application.Reporting;

public sealed class ReportingService(
    IReportingRepository repository,
    IUserRoleRepository userRoleRepository,
    IConsultantIdentityRepository consultantIdentityRepository,
    IManagerIdentityRepository managerIdentityRepository,
    TimeProvider timeProvider) : IReportingService
{
    public async Task<DashboardSummary?> GetDashboardAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(actorUserId, cancellationToken);

        if (roles.Contains(RoleNames.Administrator, StringComparer.Ordinal))
        {
            var query = DefaultReportQuery(today);
            return await BuildDashboardAsync("Administrator", await repository.GetDashboardForAdminAsync(today, cancellationToken), query, [], cancellationToken);
        }

        if (roles.Contains(RoleNames.Manager, StringComparer.Ordinal))
        {
            var managerId = await managerIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            if (managerId is null)
            {
                return null;
            }

            var query = DefaultReportQuery(today) with { ManagerId = managerId };
            return await BuildDashboardAsync("Manager", await repository.GetDashboardForManagerAsync(managerId.Value, today, cancellationToken), query, [], cancellationToken);
        }

        if (roles.Contains(RoleNames.Consultant, StringComparer.Ordinal))
        {
            var consultantId = await consultantIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            if (consultantId is null)
            {
                return null;
            }

            var query = DefaultReportQuery(today) with { ConsultantId = consultantId };
            var snapshots = await repository.GetRecentLeaveRequestsAsync(query, cancellationToken);

            return await BuildDashboardAsync("Consultant", await repository.GetDashboardForConsultantAsync(consultantId.Value, today, cancellationToken), query, snapshots, cancellationToken);
        }

        return null;
    }

    public async Task<ReportsResult?> GetReportsAsync(Guid actorUserId, ReportQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(query);
        if (!Validate(normalized).IsValid)
        {
            return null;
        }

        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(actorUserId, cancellationToken);
        ReportQuery scoped;
        if (roles.Contains(RoleNames.Administrator, StringComparer.Ordinal))
        {
            scoped = normalized;
        }
        else if (roles.Contains(RoleNames.Manager, StringComparer.Ordinal))
        {
            var managerId = await managerIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            if (managerId is null)
            {
                return null;
            }

            scoped = normalized with { ManagerId = managerId, ConsultantId = null };
        }
        else
        {
            return null;
        }

        var consultantUsage = await repository.GetConsultantLeaveUsageAsync(scoped, cancellationToken);
        var monthlyActivity = await repository.GetMonthlyLeaveActivityAsync(scoped, cancellationToken);
        var teamUsage = await repository.GetTeamLeaveUsageAsync(scoped, cancellationToken);
        var peakLeaveDays = await repository.GetPeakLeaveDaysAsync(scoped, cancellationToken);
        var statusDistribution = await repository.GetStatusDistributionAsync(scoped, cancellationToken);
        var upcomingLeaves = await repository.GetUpcomingLeavesAsync(scoped, cancellationToken);
        var upcomingHolidays = await repository.GetUpcomingHolidaysAsync(scoped, cancellationToken);

        return new ReportsResult(
            scoped,
            ReportingMapper.ToReportSummary(consultantUsage, monthlyActivity, peakLeaveDays, statusDistribution),
            consultantUsage,
            monthlyActivity,
            teamUsage,
            peakLeaveDays,
            statusDistribution,
            upcomingLeaves,
            upcomingHolidays,
            ReportingSettings.MaxRangeDays);
    }

    public ValidationResult Validate(ReportQuery query)
    {
        return ReportingValidation.Validate(Normalize(query));
    }

    private async Task<DashboardSummary> BuildDashboardAsync(
        string scope,
        DashboardData data,
        ReportQuery query,
        IReadOnlyList<LeaveRequestSnapshot> recentLeaveRequests,
        CancellationToken cancellationToken)
    {
        var upcomingLeaves = await repository.GetUpcomingLeavesAsync(query, cancellationToken);
        var upcomingHolidays = await repository.GetUpcomingHolidaysAsync(query, cancellationToken);
        return new DashboardSummary(scope, ReportingMapper.ToMetrics(data, scope), upcomingLeaves, upcomingHolidays, recentLeaveRequests);
    }

    private ReportQuery Normalize(ReportQuery query)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var startDate = query.StartDate ?? new DateOnly(today.Year, 1, 1);
        var endDate = query.EndDate ?? new DateOnly(today.Year, 12, 31);

        return query with
        {
            StartDate = startDate,
            EndDate = endDate
        };
    }

    private static ReportQuery DefaultReportQuery(DateOnly today)
    {
        return new ReportQuery(today, today.AddDays(60), null, null);
    }
}
