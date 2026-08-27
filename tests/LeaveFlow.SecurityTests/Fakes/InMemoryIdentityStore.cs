using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.People;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryIdentityStore :
    IUserAuthRepository,
    IUserRoleRepository,
    IConsultantIdentityRepository,
    IManagerIdentityRepository,
    ILoginAttemptRepository,
    IAuditLogRepository,
    IConsultantManagementRepository,
    IManagerManagementRepository,
    IManagerConsultantAssignmentRepository
{
    private readonly Dictionary<string, UserAuthRecord> _users = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, IReadOnlyList<string>> _roles = new();
    private readonly Dictionary<Guid, Guid> _consultantIds = new();
    private readonly Dictionary<Guid, Guid> _managerIds = new();
    private readonly Dictionary<Guid, ConsultantDetail> _consultants = new();
    private readonly Dictionary<Guid, ManagerDetail> _managers = new();
    private readonly HashSet<(Guid ManagerId, Guid ConsultantId)> _assignments = [];

    public List<LoginAttemptRecord> Attempts { get; } = [];
    public List<AuditLogRecord> Audits { get; } = [];

    public UserAuthRecord AddUser(UserAuthRecord user, IReadOnlyList<string> roles)
    {
        _users[EmailNormalizer.Normalize(user.Email)] = user;
        _roles[user.Id] = roles;
        return user;
    }

    public void SetConsultant(Guid userId, Guid consultantId)
    {
        _consultantIds[userId] = consultantId;
        var user = GetById(userId);
        var names = SplitDisplayName(user.DisplayName);
        _consultants[consultantId] = new ConsultantDetail(
            consultantId,
            userId,
            names.FirstName,
            names.LastName,
            user.Email,
            null,
            null,
            null,
            user.IsActive);
    }

    public void SetManager(Guid userId, Guid managerId)
    {
        _managerIds[userId] = managerId;
        var user = GetById(userId);
        var names = SplitDisplayName(user.DisplayName);
        _managers[managerId] = new ManagerDetail(
            managerId,
            userId,
            names.FirstName,
            names.LastName,
            user.Email,
            null,
            user.IsActive);
    }

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

    Task<PagedResult<ConsultantListItem>> IConsultantManagementRepository.SearchAsync(
        PeopleSearchRequest request,
        CancellationToken cancellationToken)
    {
        var items = _consultants.Values
            .Where(item => request.IsActive is null || item.IsActive == request.IsActive)
            .Where(item => string.IsNullOrWhiteSpace(request.Search)
                || item.FirstName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || item.LastName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || item.Email.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.LastName)
            .ThenBy(item => item.FirstName)
            .ToArray();

        var page = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new ConsultantListItem(
                item.Id,
                item.UserId,
                item.FirstName,
                item.LastName,
                item.Email,
                item.EmployeeNumber,
                item.Department,
                item.IsActive,
                items.Length))
            .ToArray();

        return Task.FromResult(new PagedResult<ConsultantListItem>(page, request.PageNumber, request.PageSize, items.Length));
    }

    Task<ConsultantDetail?> IConsultantManagementRepository.GetByIdAsync(Guid consultantId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_consultants.TryGetValue(consultantId, out var consultant) ? consultant : null);
    }

    Task<Guid> IConsultantManagementRepository.CreateAsync(ConsultantInput input, CancellationToken cancellationToken)
    {
        var user = AddUser(new UserAuthRecord(Guid.NewGuid(), input.Email, $"{input.FirstName} {input.LastName}", string.Empty, input.IsActive, 0, null, null), ["Consultant"]);
        var consultantId = Guid.NewGuid();
        SetConsultant(user.Id, consultantId);
        return Task.FromResult(consultantId);
    }

    Task<bool> IConsultantManagementRepository.UpdateAsync(Guid consultantId, ConsultantInput input, CancellationToken cancellationToken)
    {
        if (!_consultants.TryGetValue(consultantId, out var consultant))
        {
            return Task.FromResult(false);
        }

        _consultants[consultantId] = consultant with
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            Email = input.Email,
            EmployeeNumber = input.EmployeeNumber,
            Department = input.Department,
            StartDate = input.StartDate,
            IsActive = input.IsActive
        };

        return Task.FromResult(true);
    }

    Task<bool> IConsultantManagementRepository.SetActiveAsync(Guid consultantId, bool isActive, CancellationToken cancellationToken)
    {
        if (!_consultants.TryGetValue(consultantId, out var consultant))
        {
            return Task.FromResult(false);
        }

        _consultants[consultantId] = consultant with { IsActive = isActive };
        return Task.FromResult(true);
    }

    Task<PagedResult<ManagerListItem>> IManagerManagementRepository.SearchAsync(
        PeopleSearchRequest request,
        CancellationToken cancellationToken)
    {
        var items = _managers.Values
            .Where(item => request.IsActive is null || item.IsActive == request.IsActive)
            .Where(item => string.IsNullOrWhiteSpace(request.Search)
                || item.FirstName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || item.LastName.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || item.Email.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.LastName)
            .ThenBy(item => item.FirstName)
            .ToArray();

        var page = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new ManagerListItem(
                item.Id,
                item.UserId,
                item.FirstName,
                item.LastName,
                item.Email,
                item.Department,
                item.IsActive,
                items.Length))
            .ToArray();

        return Task.FromResult(new PagedResult<ManagerListItem>(page, request.PageNumber, request.PageSize, items.Length));
    }

    Task<ManagerDetail?> IManagerManagementRepository.GetByIdAsync(Guid managerId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_managers.TryGetValue(managerId, out var manager) ? manager : null);
    }

    Task<Guid> IManagerManagementRepository.CreateAsync(ManagerInput input, CancellationToken cancellationToken)
    {
        var user = AddUser(new UserAuthRecord(Guid.NewGuid(), input.Email, $"{input.FirstName} {input.LastName}", string.Empty, input.IsActive, 0, null, null), ["Manager"]);
        var managerId = Guid.NewGuid();
        SetManager(user.Id, managerId);
        return Task.FromResult(managerId);
    }

    Task<bool> IManagerManagementRepository.UpdateAsync(Guid managerId, ManagerInput input, CancellationToken cancellationToken)
    {
        if (!_managers.TryGetValue(managerId, out var manager))
        {
            return Task.FromResult(false);
        }

        _managers[managerId] = manager with
        {
            FirstName = input.FirstName,
            LastName = input.LastName,
            Email = input.Email,
            Department = input.Department,
            IsActive = input.IsActive
        };

        return Task.FromResult(true);
    }

    Task<bool> IManagerManagementRepository.SetActiveAsync(Guid managerId, bool isActive, CancellationToken cancellationToken)
    {
        if (!_managers.TryGetValue(managerId, out var manager))
        {
            return Task.FromResult(false);
        }

        _managers[managerId] = manager with { IsActive = isActive };
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<ManagerConsultantAssignment>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        var assignments = _assignments
            .Where(assignment => assignment.ManagerId == managerId)
            .Where(assignment => _consultants.ContainsKey(assignment.ConsultantId))
            .Select(assignment =>
            {
                var consultant = _consultants[assignment.ConsultantId];
                return new ManagerConsultantAssignment(
                    managerId,
                    consultant.Id,
                    $"{consultant.FirstName} {consultant.LastName}",
                    consultant.Email,
                    consultant.IsActive);
            })
            .ToArray();

        return Task.FromResult((IReadOnlyList<ManagerConsultantAssignment>)assignments);
    }

    public Task<bool> AssignAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_assignments.Add((managerId, consultantId)));
    }

    public Task<bool> RemoveAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_assignments.Remove((managerId, consultantId)));
    }

    private UserAuthRecord GetById(Guid userId)
    {
        return _users.Values.Single(user => user.Id == userId);
    }

    private void Replace(UserAuthRecord user)
    {
        _users[EmailNormalizer.Normalize(user.Email)] = user;
    }

    private static (string FirstName, string LastName) SplitDisplayName(string displayName)
    {
        var parts = displayName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 1 ? (parts[0], "User") : (parts[0], parts[1]);
    }
}
