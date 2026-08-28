using LeaveFlow.Application.Abstractions.Calendar;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Calendar;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.UnitTests.Calendar;

public sealed class OrganizationCalendarTests
{
    [Fact]
    public void EventTypes_Should_DefineKnownCalendarTypes()
    {
        Assert.Contains(CalendarEventTypes.Leave, CalendarEventTypes.All);
        Assert.Contains(CalendarEventTypes.OrganizationHoliday, CalendarEventTypes.All);
        Assert.Contains(CalendarEventTypes.OfficialHoliday, CalendarEventTypes.All);
    }

    [Fact]
    public void Normalizer_Should_CombineDayRowsIntoRangeEvents()
    {
        var leaveId = Guid.NewGuid();
        var consultantId = Guid.NewGuid();
        OrganizationCalendarDataRow[] rows =
        [
            new(CalendarEventTypes.Leave, leaveId, "Ada leave", new DateOnly(2026, 9, 10), consultantId, "Ada", leaveId, null),
            new(CalendarEventTypes.Leave, leaveId, "Ada leave", new DateOnly(2026, 9, 11), consultantId, "Ada", leaveId, null)
        ];

        var calendarEvent = Assert.Single(CalendarEventNormalizer.Normalize(rows));

        Assert.Equal(new DateOnly(2026, 9, 10), calendarEvent.StartDate);
        Assert.Equal(new DateOnly(2026, 9, 11), calendarEvent.EndDate);
        Assert.Equal(CalendarEventTypes.Leave, calendarEvent.EventType);
    }

    [Fact]
    public void Normalizer_Should_KeepMixedLeaveAndHolidayEvents()
    {
        var leaveId = Guid.NewGuid();
        var orgHolidayId = Guid.NewGuid();
        var officialHolidayId = Guid.NewGuid();
        OrganizationCalendarDataRow[] rows =
        [
            new(CalendarEventTypes.Leave, leaveId, "Leave", new DateOnly(2026, 9, 10), Guid.NewGuid(), "Ada", leaveId, null),
            new(CalendarEventTypes.OrganizationHoliday, orgHolidayId, "Company Day", new DateOnly(2026, 9, 10), null, null, null, orgHolidayId),
            new(CalendarEventTypes.OfficialHoliday, officialHolidayId, "Public Day", new DateOnly(2026, 9, 11), null, null, null, officialHolidayId)
        ];

        var events = CalendarEventNormalizer.Normalize(rows);

        Assert.Equal(3, events.Count);
        Assert.Contains(events, item => item.EventType == CalendarEventTypes.Leave);
        Assert.Contains(events, item => item.EventType == CalendarEventTypes.OrganizationHoliday);
        Assert.Contains(events, item => item.EventType == CalendarEventTypes.OfficialHoliday);
    }

    [Fact]
    public void Normalizer_Should_ReturnEmptyCalendar_ForEmptyRows()
    {
        Assert.Empty(CalendarEventNormalizer.Normalize([]));
    }

    [Fact]
    public void Validate_Should_RejectRangesLongerThanConfiguredMaximum()
    {
        var service = CreateService();

        var result = service.Validate(new OrganizationCalendarQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 4, 1),
            OrganizationCalendarDateRange.MonthView,
            null,
            null));

        Assert.False(result.IsValid);
        Assert.Contains("EndDate", result.Errors.Keys);
    }

    [Fact]
    public async Task Calendar_Should_IncludeMixedEvents_ForAdministrator()
    {
        var actorUserId = Guid.NewGuid();
        var repository = new StubOrganizationCalendarRepository();
        var roles = new StubCalendarUserRoleRepository();
        roles.SetRoles(actorUserId, [RoleNames.Administrator]);
        repository.Rows =
        [
            new(CalendarEventTypes.Leave, Guid.NewGuid(), "Leave", new DateOnly(2026, 9, 10), Guid.NewGuid(), "Ada", Guid.NewGuid(), null),
            new(CalendarEventTypes.OrganizationHoliday, Guid.NewGuid(), "Company Day", new DateOnly(2026, 9, 10), null, null, null, Guid.NewGuid())
        ];

        var result = await CreateService(repository, roles).GetAsync(actorUserId, new OrganizationCalendarQuery(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            OrganizationCalendarDateRange.MonthView,
            null,
            null));

        Assert.NotNull(result);
        Assert.Equal(2, result.Events.Count);
    }

    [Fact]
    public async Task ConsultantQuery_Should_IgnoreTamperedConsultantAndManagerIds()
    {
        var actorUserId = Guid.NewGuid();
        var actorConsultantId = Guid.NewGuid();
        var repository = new StubOrganizationCalendarRepository();
        var roles = new StubCalendarUserRoleRepository();
        var consultants = new StubCalendarConsultantIdentityRepository();
        roles.SetRoles(actorUserId, [RoleNames.Consultant]);
        consultants.SetConsultant(actorUserId, actorConsultantId);

        var result = await CreateService(repository, roles, consultants: consultants).GetAsync(actorUserId, new OrganizationCalendarQuery(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            OrganizationCalendarDateRange.MonthView,
            Guid.NewGuid(),
            Guid.NewGuid()));

        Assert.NotNull(result);
        Assert.Equal(actorConsultantId, repository.LastConsultantId);
        Assert.Null(repository.LastConsultantQuery?.ManagerId);
        Assert.Null(repository.LastConsultantQuery?.ConsultantId);
    }

    private static OrganizationCalendarService CreateService(
        StubOrganizationCalendarRepository? repository = null,
        StubCalendarUserRoleRepository? roles = null,
        StubCalendarConsultantIdentityRepository? consultants = null,
        StubCalendarManagerIdentityRepository? managers = null)
    {
        return new OrganizationCalendarService(
            repository ?? new StubOrganizationCalendarRepository(),
            roles ?? new StubCalendarUserRoleRepository(),
            consultants ?? new StubCalendarConsultantIdentityRepository(),
            managers ?? new StubCalendarManagerIdentityRepository(),
            new FixedCalendarTimeProvider(new DateTimeOffset(2026, 9, 15, 10, 0, 0, TimeSpan.Zero)));
    }
}

internal sealed class StubOrganizationCalendarRepository : IOrganizationCalendarRepository
{
    public IReadOnlyList<OrganizationCalendarDataRow> Rows { get; set; } = [];

    public Guid? LastConsultantId { get; private set; }

    public OrganizationCalendarQuery? LastConsultantQuery { get; private set; }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForAdminAsync(OrganizationCalendarQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Rows);
    }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForManagerAsync(Guid managerId, OrganizationCalendarQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Rows);
    }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForConsultantAsync(Guid consultantId, OrganizationCalendarQuery query, CancellationToken cancellationToken = default)
    {
        LastConsultantId = consultantId;
        LastConsultantQuery = query;
        return Task.FromResult(Rows);
    }

    public Task<CalendarEventDetail?> GetDetailForAdminAsync(CalendarEventDetailQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CalendarEventDetail?>(null);
    }

    public Task<CalendarEventDetail?> GetDetailForManagerAsync(Guid managerId, CalendarEventDetailQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CalendarEventDetail?>(null);
    }

    public Task<CalendarEventDetail?> GetDetailForConsultantAsync(Guid consultantId, CalendarEventDetailQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CalendarEventDetail?>(null);
    }
}

internal sealed class StubCalendarUserRoleRepository : IUserRoleRepository
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

internal sealed class StubCalendarConsultantIdentityRepository : IConsultantIdentityRepository
{
    private readonly Dictionary<Guid, Guid> _consultantIds = new();

    public void SetConsultant(Guid userId, Guid consultantId)
    {
        _consultantIds[userId] = consultantId;
    }

    public Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_consultantIds.TryGetValue(userId, out var consultantId) ? consultantId : (Guid?)null);
    }
}

internal sealed class StubCalendarManagerIdentityRepository : IManagerIdentityRepository
{
    public Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Guid?>(null);
    }

    public Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}

internal sealed class FixedCalendarTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
