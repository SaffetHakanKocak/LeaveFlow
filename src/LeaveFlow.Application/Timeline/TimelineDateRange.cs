namespace LeaveFlow.Application.Timeline;

public static class TimelineDateRange
{
    public static IReadOnlyList<TimelineDay> GenerateDays(DateOnly startDate, DateOnly endDate, DateOnly today)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be on or before end date.", nameof(startDate));
        }

        var days = new List<TimelineDay>();
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            days.Add(new TimelineDay(
                day,
                day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                day == today));
        }

        return days;
    }

    public static int CountInclusiveDays(DateOnly startDate, DateOnly endDate)
    {
        return endDate.DayNumber - startDate.DayNumber + 1;
    }
}
