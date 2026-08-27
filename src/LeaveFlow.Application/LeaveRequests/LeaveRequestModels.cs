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
