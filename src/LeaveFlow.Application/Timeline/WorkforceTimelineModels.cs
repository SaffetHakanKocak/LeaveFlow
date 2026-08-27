namespace LeaveFlow.Application.Timeline;

public sealed record WorkforceTimelineQuery(
    DateOnly? StartDate,
    DateOnly? EndDate,
    Guid? ManagerId,
    string? ConsultantSearch,
    bool IncludeInactive,
    int PageNumber,
    int PageSize);

public sealed record WorkforceTimelineDataRow(
    Guid ConsultantId,
    string ConsultantName,
    string ConsultantEmail,
    bool ConsultantIsActive,
    DateOnly? LeaveDate,
    Guid? LeaveRequestId,
    string? Reason,
    int TotalCount);

public sealed record TimelineDay(DateOnly Date, bool IsWeekend, bool IsToday);

public sealed record TimelineCell(
    DateOnly Date,
    bool IsWeekend,
    bool IsToday,
    bool IsOnLeave,
    Guid? LeaveRequestId,
    string? Reason);

public sealed record TimelineRow(
    Guid ConsultantId,
    string ConsultantName,
    string ConsultantEmail,
    bool ConsultantIsActive,
    IReadOnlyList<TimelineCell> Cells);

public sealed record WorkforceTimelineResult(
    WorkforceTimelineQuery Query,
    IReadOnlyList<TimelineDay> Days,
    IReadOnlyList<TimelineRow> Rows,
    int TotalCount,
    int MaxRangeDays)
{
    public int PageNumber => Query.PageNumber;

    public int PageSize => Query.PageSize;

    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
