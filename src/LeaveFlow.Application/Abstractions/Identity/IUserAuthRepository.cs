using LeaveFlow.Application.Identity;

namespace LeaveFlow.Application.Abstractions.Identity;

public interface IUserAuthRepository
{
    Task<UserAuthRecord?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task UpdateLoginSuccessAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken = default);

    Task<FailedLoginUpdate> RecordFailedLoginAsync(
        Guid userId,
        int maxFailedAccessAttempts,
        int lockoutDurationMinutes,
        CancellationToken cancellationToken = default);

    Task UpdatePasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);
}
