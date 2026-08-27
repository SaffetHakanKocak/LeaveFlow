namespace LeaveFlow.Application.People;

public sealed record ConsultantListItem(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? EmployeeNumber,
    string? Department,
    bool IsActive,
    int TotalCount);

public sealed record ConsultantDetail(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? EmployeeNumber,
    string? Department,
    DateOnly? StartDate,
    bool IsActive);

public sealed record ConsultantInput(
    string FirstName,
    string LastName,
    string Email,
    string? EmployeeNumber,
    string? Department,
    DateOnly? StartDate,
    bool IsActive);
