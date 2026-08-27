namespace LeaveFlow.Application.Identity;

public sealed record LoginAttemptRecord(
    Guid? UserId,
    string? NormalizedEmail,
    string? IpAddress,
    bool Succeeded,
    string? FailureReason);
