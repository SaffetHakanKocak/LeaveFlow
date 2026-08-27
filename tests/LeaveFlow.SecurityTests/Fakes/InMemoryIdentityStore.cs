using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryIdentityStore :
    IUserAuthRepository,
    IUserRoleRepository,
    IConsultantIdentityRepository,
    IManagerIdentityRepository,
    ILoginAttemptRepository,
    IAuditLogRepository
{
    private readonly Dictionary<string, UserAuthRecord> _users = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, IReadOnlyList<string>> _roles = new();
    private readonly Dictionary<Guid, Guid> _consultantIds = new();
    private readonly Dictionary<Guid, Guid> _managerIds = new();
    private readonly HashSet<(Guid ManagerId, Guid ConsultantId)> _assignments = [];

    public List<LoginAttemptRecord> Attempts { get; } = [];
    public List<AuditLogRecord> Audits { get; } = [];

    public UserAuthRecord AddUser(UserAuthRecord user, IReadOnlyList<string> roles)
    {
        _users[EmailNormalizer.Normalize(user.Email)] = user;
        _roles[user.Id] = roles;
        return user;
    }

    public void SetConsultant(Guid userId, Guid consultantId) => _consultantIds[userId] = consultantId;

    public void SetManager(Guid userId, Guid managerId) => _managerIds[userId] = managerId;

    public void Assign(Guid managerId, Guid consultantId) => _assignments.Add((managerId, consultantId));

    public UserAuthRecord GetUser(string email) => _users[EmailNormalizer.Normalize(email)];

    public Task<UserAuthRecord?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(normalizedEmail, out var user);
        return Task.FromResult(user);
    }

    public Task UpdateLoginSuccessAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken = default)
    {
        var user = GetById(userId);
        Replace(user with { FailedLoginCount = 0, LockoutEnd = null, LastLoginAt = lastLoginAtUtc });
        return Task.CompletedTask;
    }

    public Task<FailedLoginUpdate> RecordFailedLoginAsync(
        Guid userId,
        int maxFailedAccessAttempts,
        int lockoutDurationMinutes,
        CancellationToken cancellationToken = default)
    {
        var user = GetById(userId);
        var utcNow = DateTime.UtcNow;
        var count = user.FailedLoginCount;
        DateTime? lockoutEnd = user.LockoutEnd;
        var lockoutApplied = false;

        if (lockoutEnd is not null && lockoutEnd <= utcNow)
        {
            count = 0;
            lockoutEnd = null;
        }

        count++;
        if (count >= maxFailedAccessAttempts)
        {
            lockoutEnd = utcNow.AddMinutes(lockoutDurationMinutes);
            lockoutApplied = true;
        }

        Replace(user with { FailedLoginCount = count, LockoutEnd = lockoutEnd });
        return Task.FromResult(new FailedLoginUpdate(count, lockoutEnd, lockoutApplied));
    }

    public Task UpdatePasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        var user = GetById(userId);
        Replace(user with { PasswordHash = passwordHash });
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_roles.TryGetValue(userId, out var roles) ? roles : Array.Empty<string>());
    }

    Task<Guid?> IConsultantIdentityRepository.GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_consultantIds.TryGetValue(userId, out var id) ? id : (Guid?)null);
    }

    Task<Guid?> IManagerIdentityRepository.GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_managerIds.TryGetValue(userId, out var id) ? id : (Guid?)null);
    }

    public Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_assignments.Contains((managerId, consultantId)));
    }

    public Task InsertAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default)
    {
        Attempts.Add(record);
        return Task.CompletedTask;
    }

    public Task InsertAsync(AuditLogRecord record, CancellationToken cancellationToken = default)
    {
        Audits.Add(record);
        return Task.CompletedTask;
    }

    private UserAuthRecord GetById(Guid userId)
    {
        return _users.Values.Single(user => user.Id == userId);
    }

    private void Replace(UserAuthRecord user)
    {
        _users[EmailNormalizer.Normalize(user.Email)] = user;
    }
}
