namespace LeaveFlow.Application.Holidays;

public static class HolidayDateRange
{
    public static IReadOnlyList<DateOnly> GenerateInclusive(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be on or before end date.", nameof(startDate));
        }

        var days = new List<DateOnly>();
        for (var day = startDate; day <= endDate; day = day.AddDays(1))
        {
            days.Add(day);
        }

        return days;
    }
}
