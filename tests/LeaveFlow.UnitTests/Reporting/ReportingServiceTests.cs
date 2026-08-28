using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Reporting;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.UnitTests.Reporting;

public sealed class ReportingServiceTests
{
    [Fact]
    public void Validate_Should_RejectRangesLongerThanConfiguredMaximum()
    {
        var service = CreateService();

        var result = service.Validate(new ReportQuery(new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 2), null, null));

        Assert.False(result.IsValid);
        Assert.Contains("EndDate", result.Errors.Keys);
    }

    [Fact]
    public void Mapper_Should_CreateDashboardMetrics_ForManager()
    {
        var data = new DashboardData(4, 0, 3, 2, 5, null, null, null, null);

        var metrics = ReportingMapper.ToMetrics(data, "Manager");

        Assert.Contains(metrics, metric => metric.Label == "Ekip danışmanları" && metric.Value == 4);
        Assert.Contains(metrics, metric => metric.Label == "Bekleyen ekip talepleri" && metric.Value == 3);
    }

    [Fact]
    public void Mapper_Should_CreateReportSummary_FromAggregateRows()
    {
        var metrics = ReportingMapper.ToReportSummary(
            [new LeaveUsageReportRow(Guid.NewGuid(), "Ada", 3), new LeaveUsageReportRow(Guid.NewGuid(), "Grace", 2)],
            [new MonthlyLeaveActivityRow(2026, 9, 5, 4)],
            [new PeakLeaveDay(new DateOnly(2026, 9, 10), 2)],
            [new StatusDistributionItem("Pending", 7)]);

        Assert.Contains(metrics, metric => metric.Label == "Onaylı izin günleri" && metric.Value == 5);
        Assert.Contains(metrics, metric => metric.Label == "Bekleyen talepler" && metric.Value == 7);
    }

    [Fact]
    public async Task Reports_Should_ReturnNull_ForConsultantOrganizationReports()
    {
        var actorUserId = Guid.NewGuid();
        var roles = new StubReportingUserRoleRepository();
        roles.SetRoles(actorUserId, [RoleNames.Consultant]);
        var service = CreateService(roles: roles);

        var result = await service.GetReportsAsync(actorUserId, new ReportQuery(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null, null));

        Assert.Null(result);
    }

    [Fact]
    public async Task ManagerReports_Should_IgnoreTamperedManagerAndConsultantFilters()
    {
        var actorUserId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var repository = new StubReportingRepository();
        var roles = new StubReportingUserRoleRepository();
        var managers = new StubReportingManagerIdentityRepository();
        roles.SetRoles(actorUserId, [RoleNames.Manager]);
        managers.SetManager(actorUserId, managerId);

        var result = await CreateService(repository, roles, managers: managers).GetReportsAsync(actorUserId, new ReportQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            Guid.NewGuid(),
            Guid.NewGuid()));

        Assert.NotNull(result);
        Assert.Equal(managerId, repository.LastReportQuery?.ManagerId);
        Assert.Null(repository.LastReportQuery?.ConsultantId);
    }

    [Fact]
    public async Task EmptyReports_Should_MapToEmptyResult()
    {
        var actorUserId = Guid.NewGuid();
        var roles = new StubReportingUserRoleRepository();
        roles.SetRoles(actorUserId, [RoleNames.Administrator]);

        var result = await CreateService(roles: roles).GetReportsAsync(actorUserId, new ReportQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            null,
            null));

        Assert.NotNull(result);
        Assert.Empty(result.ConsultantLeaveUsage);
        Assert.Empty(result.UpcomingLeaves);
    }

    private static ReportingService CreateService(
        StubReportingRepository? repository = null,
        StubReportingUserRoleRepository? roles = null,
        StubReportingConsultantIdentityRepository? consultants = null,
        StubReportingManagerIdentityRepository? managers = null)
    {
        return new ReportingService(
            repository ?? new StubReportingRepository(),
            roles ?? new StubReportingUserRoleRepository(),
            consultants ?? new StubReportingConsultantIdentityRepository(),
            managers ?? new StubReportingManagerIdentityRepository(),
            new FixedReportingTimeProvider(new DateTimeOffset(2026, 8, 28, 10, 0, 0, TimeSpan.Zero)));
    }
}

internal sealed class StubReportingRepository : IReportingRepository
{
    public ReportQuery? LastReportQuery { get; private set; }

    public Task<DashboardData> GetDashboardForAdminAsync(DateOnly today, CancellationToken cancellationToken = default) => Task.FromResult(new DashboardData(0, 0, 0, 0, 0, null, null, null, null));

    public Task<DashboardData> GetDashboardForManagerAsync(Guid managerId, DateOnly today, CancellationToken cancellationToken = default) => Task.FromResult(new DashboardData(0, 0, 0, 0, 0, null, null, null, null));

    public Task<DashboardData> GetDashboardForConsultantAsync(Guid consultantId, DateOnly today, CancellationToken cancellationToken = default) => Task.FromResult(new DashboardData(0, 0, 0, 0, 0, null, null, null, null));

    public Task<IReadOnlyList<LeaveUsageReportRow>> GetConsultantLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        LastReportQuery = query;
        return Task.FromResult<IReadOnlyList<LeaveUsageReportRow>>([]);
    }

    public Task<IReadOnlyList<MonthlyLeaveActivityRow>> GetMonthlyLeaveActivityAsync(ReportQuery query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MonthlyLeaveActivityRow>>([]);

    public Task<IReadOnlyList<TeamLeaveUsageRow>> GetTeamLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TeamLeaveUsageRow>>([]);

    public Task<IReadOnlyList<PeakLeaveDay>> GetPeakLeaveDaysAsync(ReportQuery query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<PeakLeaveDay>>([]);

    public Task<IReadOnlyList<StatusDistributionItem>> GetStatusDistributionAsync(ReportQuery query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<StatusDistributionItem>>([]);

    public Task<IReadOnlyList<UpcomingLeaveItem>> GetUpcomingLeavesAsync(ReportQuery query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<UpcomingLeaveItem>>([]);

    public Task<IReadOnlyList<UpcomingHolidayItem>> GetUpcomingHolidaysAsync(ReportQuery query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<UpcomingHolidayItem>>([]);

    public Task<IReadOnlyList<LeaveRequestSnapshot>> GetRecentLeaveRequestsAsync(ReportQuery query, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<LeaveRequestSnapshot>>([]);
}

internal sealed class StubReportingUserRoleRepository : IUserRoleRepository
{
    private readonly Dictionary<Guid, IReadOnlyList<string>> _roles = new();

    public void SetRoles(Guid userId, IReadOnlyList<string> roles) => _roles[userId] = roles;

    public Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_roles.TryGetValue(userId, out var roles) ? roles : Array.Empty<string>());
    }
}

internal sealed class StubReportingConsultantIdentityRepository : IConsultantIdentityRepository
{
    public Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(null);
}

internal sealed class StubReportingManagerIdentityRepository : IManagerIdentityRepository
{
    private readonly Dictionary<Guid, Guid> _managerIds = new();

    public void SetManager(Guid userId, Guid managerId) => _managerIds[userId] = managerId;

    public Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_managerIds.TryGetValue(userId, out var managerId) ? managerId : (Guid?)null);
    }

    public Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default) => Task.FromResult(false);
}

internal sealed class FixedReportingTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
