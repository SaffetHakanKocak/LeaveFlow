namespace LeaveFlow.Application.Calendar;

public static class OrganizationCalendarDateRange
{
    public const string MonthView = "Month";
    public const string WeekView = "Week";

    public static IReadOnlyList<CalendarDay> GenerateDays(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly today,
        string viewMode)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be on or before end date.", nameof(startDate));
        }

        var visibleStart = viewMode == WeekView ? startDate : StartOfWeek(new DateOnly(startDate.Year, startDate.Month, 1));
        var visibleEnd = viewMode == WeekView ? endDate : EndOfWeek(new DateOnly(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month)));

        var days = new List<CalendarDay>();
        for (var day = visibleStart; day <= visibleEnd; day = day.AddDays(1))
        {
            days.Add(new CalendarDay(
                day,
                day >= startDate && day <= endDate,
                day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                day == today));
        }

        return days;
    }

    public static int CountInclusiveDays(DateOnly startDate, DateOnly endDate)
    {
        return endDate.DayNumber - startDate.DayNumber + 1;
    }

    public static DateOnly StartOfWeek(DateOnly date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }

    public static DateOnly EndOfWeek(DateOnly date)
    {
        return StartOfWeek(date).AddDays(6);
    }
}
