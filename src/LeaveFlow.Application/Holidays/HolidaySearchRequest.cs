namespace LeaveFlow.Application.Holidays;

public sealed record HolidaySearchRequest(
    string? Search,
    bool? IsActive,
    DateOnly? FromDate,
    DateOnly? ToDate,
    int PageNumber,
    int PageSize);
