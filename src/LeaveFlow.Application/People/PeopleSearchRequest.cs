namespace LeaveFlow.Application.People;

public sealed record PeopleSearchRequest(
    string? Search,
    bool? IsActive,
    int PageNumber,
    int PageSize);
