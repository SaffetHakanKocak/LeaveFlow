namespace LeaveFlow.Application.Identity;

public sealed record UserSummary(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive);
