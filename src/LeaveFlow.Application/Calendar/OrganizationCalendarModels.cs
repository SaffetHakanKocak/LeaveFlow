namespace LeaveFlow.Application.Calendar;

public sealed record OrganizationCalendarQuery(
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ViewMode,
    Guid? ManagerId,
    Guid? ConsultantId);

public sealed record CalendarEventDetailQuery(string EventType, Guid EventId);

public sealed record OrganizationCalendarDataRow(
    string EventType,
    Guid EventId,
    string Title,
    DateOnly EventDate,
    Guid? ConsultantId,
    string? ConsultantName,
    Guid? LeaveRequestId,
    Guid? HolidayDefinitionId);

public sealed record CalendarEvent(
    string EventType,
    Guid EventId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ConsultantId,
    string? ConsultantName,
    Guid? LeaveRequestId,
    Guid? HolidayDefinitionId);

public sealed record CalendarDay(DateOnly Date, bool IsCurrentMonth, bool IsWeekend, bool IsToday);

public sealed record OrganizationCalendarResult(
    OrganizationCalendarQuery Query,
    IReadOnlyList<CalendarDay> Days,
    IReadOnlyList<CalendarEvent> Events,
    int MaxRangeDays);

public sealed record CalendarEventDetail(
    string EventType,
    Guid EventId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ConsultantId,
    string? ConsultantName,
    Guid? LeaveRequestId,
    Guid? HolidayDefinitionId,
    string? Summary);
