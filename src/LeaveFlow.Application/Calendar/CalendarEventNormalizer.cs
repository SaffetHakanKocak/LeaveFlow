namespace LeaveFlow.Application.Calendar;

public static class CalendarEventNormalizer
{
    public static IReadOnlyList<CalendarEvent> Normalize(IReadOnlyList<OrganizationCalendarDataRow> rows)
    {
        return rows
            .Where(row => CalendarEventTypes.All.Contains(row.EventType))
            .GroupBy(row => new
            {
                row.EventType,
                row.EventId,
                row.Title,
                row.ConsultantId,
                row.ConsultantName,
                row.LeaveRequestId,
                row.HolidayDefinitionId
            })
            .Select(group => new CalendarEvent(
                group.Key.EventType,
                group.Key.EventId,
                group.Key.Title,
                group.Min(row => row.EventDate),
                group.Max(row => row.EventDate),
                group.Key.ConsultantId,
                group.Key.ConsultantName,
                group.Key.LeaveRequestId,
                group.Key.HolidayDefinitionId))
            .OrderBy(calendarEvent => calendarEvent.StartDate)
            .ThenBy(calendarEvent => EventTypeSort(calendarEvent.EventType))
            .ThenBy(calendarEvent => calendarEvent.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static int EventTypeSort(string eventType)
    {
        return eventType switch
        {
            CalendarEventTypes.OrganizationHoliday => 0,
            CalendarEventTypes.OfficialHoliday => 1,
            CalendarEventTypes.Leave => 2,
            _ => 9
        };
    }
}
