namespace LeaveFlow.Application.People;

public sealed record ManagerListItem(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    bool IsActive,
    int TotalCount);

public sealed record ManagerDetail(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    bool IsActive);

public sealed record ManagerInput(
    string FirstName,
    string LastName,
    string Email,
    string? Department,
    bool IsActive);

public sealed record ManagerConsultantAssignment(
    Guid ManagerId,
    Guid ConsultantId,
    string ConsultantName,
    string ConsultantEmail,
    bool IsActive);
