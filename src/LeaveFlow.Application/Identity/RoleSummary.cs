namespace LeaveFlow.Application.Identity;

public sealed record RoleSummary(
    int Id,
    string Name,
    string Description,
    bool IsActive);
