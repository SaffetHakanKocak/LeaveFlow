namespace LeaveFlow.Application.Calendar;

public static class CalendarEventTypes
{
    public const string Leave = nameof(Leave);
    public const string OrganizationHoliday = nameof(OrganizationHoliday);
    public const string OfficialHoliday = nameof(OfficialHoliday);

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Leave,
        OrganizationHoliday,
        OfficialHoliday
    };
}
