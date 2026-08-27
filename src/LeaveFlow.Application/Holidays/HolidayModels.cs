namespace LeaveFlow.Application.Holidays;

public sealed record OrganizationHolidayListItem(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive,
    int DayCount,
    int TotalCount);

public sealed record OrganizationHolidayDetail(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive,
    IReadOnlyList<DateOnly> Days);

public sealed record OrganizationHolidayInput(
    string Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive);

public sealed record OfficialHolidayListItem(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    int DayCount,
    int TotalCount);

public sealed record OfficialHolidayDetail(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<DateOnly> Days);

public sealed record OfficialHolidayInput(
    string Name,
    DateOnly? StartDate,
    DateOnly? EndDate);
