using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Timeline;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.UnitTests.Timeline;

public sealed class WorkforceTimelineTests
{
    private static readonly DateOnly Today = new(2026, 8, 27);

    [Fact]
    public void GenerateDays_Should_CreateInclusiveHeaders_AndMarkWeekendAndToday()
    {
        var days = TimelineDateRange.GenerateDays(new DateOnly(2026, 8, 27), new DateOnly(2026, 8, 30), Today);

        Assert.Equal(
            [new DateOnly(2026, 8, 27), new DateOnly(2026, 8, 28), new DateOnly(2026, 8, 29), new DateOnly(2026, 8, 30)],
            days.Select(day => day.Date).ToArray());
        Assert.True(days[0].IsToday);
        Assert.False(days[1].IsWeekend);
        Assert.True(days[2].IsWeekend);
        Assert.True(days[3].IsWeekend);
    }

    [Fact]
    public void MatrixBuilder_Should_MapLeaveDays_AndKeepAvailableCells()
    {
        var consultantId = Guid.NewGuid();
        var leaveRequestId = Guid.NewGuid();
        var days = TimelineDateRange.GenerateDays(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3), Today);
        WorkforceTimelineDataRow[] dataRows =
        [
            new(consultantId, "Ada Lovelace", "ada@example.test", true, new DateOnly(2026, 9, 2), leaveRequestId, "Vacation", 1)
        ];

        var rows = WorkforceTimelineMatrixBuilder.Build(dataRows, days);

        var row = Assert.Single(rows);
        Assert.Equal("Ada Lovelace", row.ConsultantName);
        Assert.False(row.Cells[0].IsOnLeave);
        Assert.True(row.Cells[1].IsOnLeave);
        Assert.Equal(leaveRequestId, row.Cells[1].LeaveRequestId);
        Assert.Equal("Vacation", row.Cells[1].Reason);
        Assert.False(row.Cells[2].IsOnLeave);
    }

    [Fact]
    public void MatrixBuilder_Should_CreateRow_WhenConsultantHasNoLeaveDays()
    {
        var consultantId = Guid.NewGuid();
        var days = TimelineDateRange.GenerateDays(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 2), Today);
        WorkforceTimelineDataRow[] dataRows =
        [
            new(consultantId, "Grace Hopper", "grace@example.test", true, null, null, null, 1)
        ];

        var row = Assert.Single(WorkforceTimelineMatrixBuilder.Build(dataRows, days));

        Assert.Equal(2, row.Cells.Count);
        Assert.All(row.Cells, cell => Assert.False(cell.IsOnLeave));
    }

    [Fact]
    public void Validate_Should_RejectRangesLongerThanConfiguredMaximum()
    {
        var service = CreateService();

        var result = service.Validate(new WorkforceTimelineQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 3, 15),
            null,
            null,
            false,
            1,
            25));

        Assert.False(result.IsValid);
        Assert.Contains("EndDate", result.Errors.Keys);
    }

    [Fact]
    public async Task ManagerQuery_Should_IgnoreTamperedManagerId_AndUseActorManagerId()
    {
        var actorUserId = Guid.NewGuid();
        var actorManagerId = Guid.NewGuid();
        var tamperedManagerId = Guid.NewGuid();
        var repository = new StubTimelineRepository();
        var roles = new StubUserRoleRepository();
        var managers = new StubManagerIdentityRepository();
        roles.SetRoles(actorUserId, [RoleNames.Manager]);
        managers.SetManager(actorUserId, actorManagerId);
        var service = CreateService(repository, roles, managers);

        var result = await service.GetAsync(actorUserId, new WorkforceTimelineQuery(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 5),
            tamperedManagerId,
            "  ada  ",
            false,
            0,
            500));

        Assert.NotNull(result);
        Assert.Equal(actorManagerId, repository.LastManagerId);
        Assert.Null(repository.LastManagerQuery?.ManagerId);
        Assert.Equal("ada", repository.LastManagerQuery?.ConsultantSearch);
        Assert.Equal(1, repository.LastManagerQuery?.PageNumber);
        Assert.Equal(100, repository.LastManagerQuery?.PageSize);
    }

    [Fact]
    public async Task Consultant_Should_NotReceiveGlobalTimeline()
    {
        var actorUserId = Guid.NewGuid();
        var roles = new StubUserRoleRepository();
        roles.SetRoles(actorUserId, [RoleNames.Consultant]);
        var service = CreateService(roles: roles);

        var result = await service.GetAsync(actorUserId, new WorkforceTimelineQuery(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 5),
            null,
            null,
            false,
            1,
            25));

        Assert.Null(result);
    }

    private static WorkforceTimelineService CreateService(
        StubTimelineRepository? repository = null,
        StubUserRoleRepository? roles = null,
        StubManagerIdentityRepository? managers = null)
    {
        return new WorkforceTimelineService(
            repository ?? new StubTimelineRepository(),
            roles ?? new StubUserRoleRepository(),
            managers ?? new StubManagerIdentityRepository(),
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 27, 10, 0, 0, TimeSpan.Zero)));
    }
}

internal sealed class StubTimelineRepository : IWorkforceTimelineRepository
{
    public Guid? LastManagerId { get; private set; }

    public WorkforceTimelineQuery? LastManagerQuery { get; private set; }

    public Task<PagedResult<WorkforceTimelineDataRow>> GetForAdminAsync(
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PagedResult<WorkforceTimelineDataRow>([], query.PageNumber, query.PageSize, 0));
    }

    public Task<PagedResult<WorkforceTimelineDataRow>> GetForManagerAsync(
        Guid managerId,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default)
    {
        LastManagerId = managerId;
        LastManagerQuery = query;
        return Task.FromResult(new PagedResult<WorkforceTimelineDataRow>([], query.PageNumber, query.PageSize, 0));
    }
}

internal sealed class StubUserRoleRepository : IUserRoleRepository
{
    private readonly Dictionary<Guid, IReadOnlyList<string>> _roles = new();

    public void SetRoles(Guid userId, IReadOnlyList<string> roles)
    {
        _roles[userId] = roles;
    }

    public Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_roles.TryGetValue(userId, out var roles) ? roles : Array.Empty<string>());
    }
}

internal sealed class StubManagerIdentityRepository : IManagerIdentityRepository
{
    private readonly Dictionary<Guid, Guid> _managerIds = new();

    public void SetManager(Guid userId, Guid managerId)
    {
        _managerIds[userId] = managerId;
    }

    public Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_managerIds.TryGetValue(userId, out var managerId) ? managerId : (Guid?)null);
    }

    public Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
