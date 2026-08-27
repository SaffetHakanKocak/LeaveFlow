using LeaveFlow.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Application.Identity;

public sealed class LoginService(
    IUserAuthRepository userAuthRepository,
    IUserRoleRepository userRoleRepository,
    IConsultantIdentityRepository consultantIdentityRepository,
    IManagerIdentityRepository managerIdentityRepository,
    ILoginAttemptRepository loginAttemptRepository,
    IAuditLogRepository auditLogRepository,
    IPasswordHashingService passwordHashingService,
    IOptions<AuthenticationSettings> authenticationSettings,
    TimeProvider timeProvider,
    ILogger<LoginService> logger) : ILoginService
{
    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var settings = authenticationSettings.Value;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrEmpty(request.Password)
            || request.Password.Length > settings.MaxPasswordLength)
        {
            await RecordFailureAsync(
                user: null,
                normalizedEmail: string.IsNullOrWhiteSpace(request.Email) ? null : EmailNormalizer.Normalize(request.Email),
                ipAddress: request.IpAddress,
                correlationId: request.CorrelationId,
                failureReason: LoginFailureReasons.InvalidCredentials,
                cancellationToken);
            return LoginResult.Failed();
        }

        var normalizedEmail = EmailNormalizer.Normalize(request.Email);
        var user = await userAuthRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            passwordHashingService.PerformDummyVerification(request.Password);
            await RecordFailureAsync(
                user: null,
                normalizedEmail,
                request.IpAddress,
                request.CorrelationId,
                LoginFailureReasons.UserNotFound,
                cancellationToken);
            logger.LogWarning("Authentication failed for an unknown account.");
            return LoginResult.Failed();
        }

        if (IsLockedOut(user, utcNow))
        {
            VerifyOrDummy(user, request.Password);
            await RecordFailureAsync(
                user,
                normalizedEmail,
                request.IpAddress,
                request.CorrelationId,
                LoginFailureReasons.LockedOut,
                cancellationToken);
            logger.LogWarning("Authentication failed for user {UserId}.", user.Id);
            return LoginResult.Failed();
        }

        if (!user.IsActive)
        {
            VerifyOrDummy(user, request.Password);
            await RecordFailureAsync(
                user,
                normalizedEmail,
                request.IpAddress,
                request.CorrelationId,
                LoginFailureReasons.InactiveAccount,
                cancellationToken);
            logger.LogWarning("Authentication failed for user {UserId}.", user.Id);
            return LoginResult.Failed();
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            passwordHashingService.PerformDummyVerification(request.Password);
            await RecordInvalidPasswordAsync(user, normalizedEmail, request, settings, cancellationToken);
            return LoginResult.Failed();
        }

        var verification = passwordHashingService.Verify(user.PasswordHash, request.Password);
        if (verification == PasswordVerificationOutcome.Failed)
        {
            await RecordInvalidPasswordAsync(user, normalizedEmail, request, settings, cancellationToken);
            return LoginResult.Failed();
        }

        if (verification == PasswordVerificationOutcome.SuccessRehashNeeded)
        {
            var rehashed = passwordHashingService.HashPassword(request.Password);
            await userAuthRepository.UpdatePasswordHashAsync(user.Id, rehashed, cancellationToken);
        }

        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);
        var consultantId = await consultantIdentityRepository.GetIdByUserIdAsync(user.Id, cancellationToken);
        var managerId = await managerIdentityRepository.GetIdByUserIdAsync(user.Id, cancellationToken);

        await userAuthRepository.UpdateLoginSuccessAsync(user.Id, utcNow, cancellationToken);
        await loginAttemptRepository.InsertAsync(
            new LoginAttemptRecord(user.Id, normalizedEmail, request.IpAddress, true, null),
            cancellationToken);
        await auditLogRepository.InsertAsync(
            CreateAudit(user.Id, AuditActions.LoginSuccess, AuditOutcomes.Success, request.CorrelationId, null),
            cancellationToken);

        logger.LogInformation("Authentication succeeded for user {UserId}.", user.Id);

        return LoginResult.Success(
            new AuthenticatedUser(user.Id, user.Email, user.DisplayName, roles, consultantId, managerId));
    }

    public async Task RecordLogoutAsync(Guid userId, string? correlationId, CancellationToken cancellationToken = default)
    {
        await auditLogRepository.InsertAsync(
            CreateAudit(userId, AuditActions.Logout, AuditOutcomes.Success, correlationId, null),
            cancellationToken);

        logger.LogInformation("User {UserId} signed out.", userId);
    }

    private async Task RecordInvalidPasswordAsync(
        UserAuthRecord user,
        string normalizedEmail,
        LoginRequest request,
        AuthenticationSettings settings,
        CancellationToken cancellationToken)
    {
        var update = await userAuthRepository.RecordFailedLoginAsync(
            user.Id,
            settings.MaxFailedAccessAttempts,
            settings.LockoutDurationMinutes,
            cancellationToken);

        await loginAttemptRepository.InsertAsync(
            new LoginAttemptRecord(user.Id, normalizedEmail, request.IpAddress, false, LoginFailureReasons.InvalidCredentials),
            cancellationToken);

        if (update.LockoutApplied)
        {
            await auditLogRepository.InsertAsync(
                CreateAudit(user.Id, AuditActions.Lockout, AuditOutcomes.Denied, request.CorrelationId, """{"reason":"InvalidCredentials","lockoutApplied":true}"""),
                cancellationToken);
            logger.LogWarning("User {UserId} was locked out after failed authentication attempts.", user.Id);
        }
        else
        {
            await auditLogRepository.InsertAsync(
                CreateAudit(user.Id, AuditActions.LoginFailure, AuditOutcomes.Failure, request.CorrelationId, """{"reason":"InvalidCredentials"}"""),
                cancellationToken);
            logger.LogWarning("Authentication failed for user {UserId}.", user.Id);
        }
    }

    private async Task RecordFailureAsync(
        UserAuthRecord? user,
        string? normalizedEmail,
        string? ipAddress,
        string? correlationId,
        string failureReason,
        CancellationToken cancellationToken)
    {
        await loginAttemptRepository.InsertAsync(
            new LoginAttemptRecord(user?.Id, normalizedEmail, ipAddress, false, failureReason),
            cancellationToken);

        await auditLogRepository.InsertAsync(
            CreateAudit(
                user?.Id,
                AuditActions.LoginFailure,
                AuditOutcomes.Failure,
                correlationId,
                $"{{\"reason\":\"{failureReason}\"}}"),
            cancellationToken);
    }

    private void VerifyOrDummy(UserAuthRecord user, string password)
    {
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            passwordHashingService.PerformDummyVerification(password);
            return;
        }

        passwordHashingService.Verify(user.PasswordHash, password);
    }

    private static bool IsLockedOut(UserAuthRecord user, DateTime utcNow)
    {
        if (user.LockoutEnd is null)
        {
            return false;
        }

        var lockoutEndUtc = DateTime.SpecifyKind(user.LockoutEnd.Value, DateTimeKind.Utc);
        return lockoutEndUtc > utcNow;
    }

    private static AuditLogRecord CreateAudit(
        Guid? actorUserId,
        string action,
        string outcome,
        string? correlationId,
        string? metadataJson)
    {
        return new AuditLogRecord(
            actorUserId,
            action,
            "User",
            actorUserId?.ToString(),
            outcome,
            correlationId,
            metadataJson);
    }
}
