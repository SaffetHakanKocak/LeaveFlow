namespace LeaveFlow.Application.LeaveRequests;

public sealed record LeaveRequestListItem(
    Guid Id,
    Guid ConsultantId,
    string Reason,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTime CreatedAt,
    int TotalCount);

public sealed record LeaveRequestDetail(
    Guid Id,
    Guid ConsultantId,
    string Reason,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    Guid? ReviewedBy,
    string? ReviewNote);

public sealed record LeaveRequestInput(
    string Reason,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record LeaveRequestCreateResult(
    bool Succeeded,
    Guid? LeaveRequestId,
    string? ErrorCode)
{
    public static LeaveRequestCreateResult Success(Guid leaveRequestId) => new(true, leaveRequestId, null);

    public static LeaveRequestCreateResult Failure(string errorCode) => new(false, null, errorCode);
}

public sealed record PendingLeaveRequestListItem(
    Guid Id,
    Guid ConsultantId,
    string ConsultantName,
    string ConsultantEmail,
    string Reason,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTime CreatedAt,
    int TotalCount);

public sealed record LeaveRequestReviewDetail(
    Guid Id,
    Guid ConsultantId,
    string ConsultantName,
    string ConsultantEmail,
    string Reason,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    Guid? ReviewedBy,
    string? ReviewNote)
{
    public int CalendarDayCount => EndDate.DayNumber - StartDate.DayNumber + 1;
}

public sealed record LeaveConflict(
    Guid ConsultantId,
    string ConsultantName,
    string ConsultantEmail,
    DateOnly ConflictDate);

public sealed record LeaveReviewSearchRequest(
    Guid? ConsultantId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string? Status,
    int PageNumber,
    int PageSize);

public sealed record ReviewDecisionInput(string? ReviewNote);

public sealed record ReviewDecisionResult(bool Succeeded, string? ErrorCode)
{
    public static ReviewDecisionResult Success() => new(true, null);

    public static ReviewDecisionResult Failure(string errorCode) => new(false, errorCode);
}
