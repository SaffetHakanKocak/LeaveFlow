namespace LeaveFlow.Application.Identity;

public sealed record UserAuthRecord(
    Guid Id,
    string Email,
    string DisplayName,
    string? PasswordHash,
    bool IsActive,
    int FailedLoginCount,
    DateTime? LockoutEnd,
    DateTime? LastLoginAt);
