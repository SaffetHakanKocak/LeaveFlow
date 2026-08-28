using LeaveFlow.Application.Abstractions.Calendar;
using LeaveFlow.Application.Calendar;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryOrganizationCalendarStore : IOrganizationCalendarRepository
{
    private readonly List<CalendarConsultant> _consultants = [];
    private readonly List<CalendarLeave> _leaves = [];
    private readonly List<CalendarHoliday> _organizationHolidays = [];
    private readonly List<CalendarHoliday> _officialHolidays = [];
    private readonly HashSet<(Guid ManagerId, Guid ConsultantId)> _assignments = [];

    public void AddConsultant(Guid consultantId, string name)
    {
        _consultants.RemoveAll(consultant => consultant.Id == consultantId);
        _consultants.Add(new CalendarConsultant(consultantId, name));
    }

    public void AssignManager(Guid managerId, Guid consultantId)
    {
        _assignments.Add((managerId, consultantId));
    }

    public Guid AddLeave(Guid consultantId, string reason, DateOnly startDate, DateOnly endDate, bool approved = true)
    {
        var id = Guid.NewGuid();
        _leaves.Add(new CalendarLeave(id, consultantId, reason, startDate, endDate, approved));
        return id;
    }

    public Guid AddOrganizationHoliday(string name, DateOnly startDate, DateOnly endDate, bool active = true)
    {
        var id = Guid.NewGuid();
        _organizationHolidays.Add(new CalendarHoliday(id, name, startDate, endDate, active, null));
        return id;
    }

    public Guid AddOfficialHoliday(string name, DateOnly startDate, DateOnly endDate, bool active = true)
    {
        var id = Guid.NewGuid();
        _officialHolidays.Add(new CalendarHoliday(id, name, startDate, endDate, active, "ZZ"));
        return id;
    }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForAdminAsync(
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetRows(query, consultant => query.ManagerId is null
            || _assignments.Contains((query.ManagerId.Value, consultant.Id)), includeConsultantNames: true));
    }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForManagerAsync(
        Guid managerId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetRows(query, consultant => _assignments.Contains((managerId, consultant.Id)), includeConsultantNames: true));
    }

    public Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForConsultantAsync(
        Guid consultantId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetRows(
            query with { ConsultantId = consultantId },
            consultant => consultant.Id == consultantId,
            includeConsultantNames: false));
    }

    public Task<CalendarEventDetail?> GetDetailForAdminAsync(
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetDetail(query, _ => true, includeConsultantName: true));
    }

    public Task<CalendarEventDetail?> GetDetailForManagerAsync(
        Guid managerId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetDetail(
            query,
            leave => _assignments.Contains((managerId, leave.ConsultantId)),
            includeConsultantName: true));
    }

    public Task<CalendarEventDetail?> GetDetailForConsultantAsync(
        Guid consultantId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetDetail(
            query,
            leave => leave.ConsultantId == consultantId,
            includeConsultantName: false));
    }

    private IReadOnlyList<OrganizationCalendarDataRow> GetRows(
        OrganizationCalendarQuery query,
        Func<CalendarConsultant, bool> consultantScope,
        bool includeConsultantNames)
    {
        var rows = new List<OrganizationCalendarDataRow>();

        foreach (var leave in _leaves.Where(leave => leave.Approved))
        {
            var consultant = _consultants.Single(item => item.Id == leave.ConsultantId);
            if (!consultantScope(consultant) || (query.ConsultantId is not null && consultant.Id != query.ConsultantId))
            {
                continue;
            }

            foreach (var day in DaysInRange(leave.StartDate, leave.EndDate, query.StartDate!.Value, query.EndDate!.Value))
            {
                rows.Add(new OrganizationCalendarDataRow(
                    CalendarEventTypes.Leave,
                    leave.Id,
                    includeConsultantNames ? $"{consultant.Name} leave" : "My leave",
                    day,
                    consultant.Id,
                    includeConsultantNames ? consultant.Name : null,
                    leave.Id,
                    null));
            }
        }

        rows.AddRange(ToHolidayRows(_organizationHolidays, CalendarEventTypes.OrganizationHoliday, query));
        rows.AddRange(ToHolidayRows(_officialHolidays, CalendarEventTypes.OfficialHoliday, query));
        return rows;
    }

    private CalendarEventDetail? GetDetail(
        CalendarEventDetailQuery query,
        Func<CalendarLeave, bool> leaveScope,
        bool includeConsultantName)
    {
        if (query.EventType == CalendarEventTypes.Leave)
        {
            var leave = _leaves.SingleOrDefault(item => item.Id == query.EventId && item.Approved && leaveScope(item));
            if (leave is null)
            {
                return null;
            }

            var consultant = _consultants.Single(item => item.Id == leave.ConsultantId);
            return new CalendarEventDetail(
                CalendarEventTypes.Leave,
                leave.Id,
                includeConsultantName ? $"{consultant.Name} leave" : "My leave",
                leave.StartDate,
                leave.EndDate,
                leave.ConsultantId,
                includeConsultantName ? consultant.Name : null,
                leave.Id,
                null,
                leave.Reason);
        }

        var holiday = query.EventType == CalendarEventTypes.OrganizationHoliday
            ? _organizationHolidays.SingleOrDefault(item => item.Id == query.EventId && item.Active)
            : _officialHolidays.SingleOrDefault(item => item.Id == query.EventId && item.Active);

        if (holiday is null)
        {
            return null;
        }

        return new CalendarEventDetail(
            query.EventType,
            holiday.Id,
            holiday.Name,
            holiday.StartDate,
            holiday.EndDate,
            null,
            null,
            null,
            holiday.Id,
            holiday.Summary);
    }

    private static IEnumerable<OrganizationCalendarDataRow> ToHolidayRows(
        IEnumerable<CalendarHoliday> holidays,
        string eventType,
        OrganizationCalendarQuery query)
    {
        foreach (var holiday in holidays.Where(item => item.Active))
        {
            foreach (var day in DaysInRange(holiday.StartDate, holiday.EndDate, query.StartDate!.Value, query.EndDate!.Value))
            {
                yield return new OrganizationCalendarDataRow(
                    eventType,
                    holiday.Id,
                    holiday.Name,
                    day,
                    null,
                    null,
                    null,
                    holiday.Id);
            }
        }
    }

    private static IEnumerable<DateOnly> DaysInRange(DateOnly startDate, DateOnly endDate, DateOnly queryStart, DateOnly queryEnd)
    {
        var first = startDate > queryStart ? startDate : queryStart;
        var last = endDate < queryEnd ? endDate : queryEnd;
        for (var day = first; day <= last; day = day.AddDays(1))
        {
            yield return day;
        }
    }

    private sealed record CalendarConsultant(Guid Id, string Name);

    private sealed record CalendarLeave(Guid Id, Guid ConsultantId, string Reason, DateOnly StartDate, DateOnly EndDate, bool Approved);

    private sealed record CalendarHoliday(Guid Id, string Name, DateOnly StartDate, DateOnly EndDate, bool Active, string? Summary);
}
