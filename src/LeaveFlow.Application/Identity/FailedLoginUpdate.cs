namespace LeaveFlow.Application.Identity;

public sealed record FailedLoginUpdate(
    int FailedLoginCount,
    DateTime? LockoutEnd,
    bool LockoutApplied);
