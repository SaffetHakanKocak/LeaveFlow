namespace LeaveFlow.Application.Identity;

public sealed record AuthenticatedUser(
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles,
    Guid? ConsultantId,
    Guid? ManagerId);
