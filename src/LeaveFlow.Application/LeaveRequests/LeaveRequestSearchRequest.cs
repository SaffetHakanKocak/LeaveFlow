namespace LeaveFlow.Application.LeaveRequests;

public sealed record LeaveRequestSearchRequest(
    string? Status,
    DateOnly? FromDate,
    DateOnly? ToDate,
    int PageNumber,
    int PageSize);
