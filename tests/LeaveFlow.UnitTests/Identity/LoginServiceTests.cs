using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Domain.Identity;
using LeaveFlow.Infrastructure.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LeaveFlow.UnitTests.Identity;

public sealed class LoginServiceTests
{
    private const string Password = "Test.Passw0rd!";
    private readonly AspNetPasswordHashingService _hasher = new();

    [Fact]
    public async Task ValidUser_Should_LoginSuccessfully()
    {
        var fixture = CreateFixture(active: true);
        var result = await fixture.Service.LoginAsync(new LoginRequest(fixture.Email, Password, "127.0.0.1", "corr"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.User);
        Assert.Equal(0, fixture.Store.User.FailedLoginCount);
        Assert.Null(fixture.Store.User.LockoutEnd);
        Assert.Contains(fixture.Store.Attempts, attempt => attempt.Succeeded);
        Assert.Contains(fixture.Store.Audits, audit => audit.Action == AuditActions.LoginSuccess);
    }

    [Fact]
    public async Task InvalidPassword_Should_ReturnGenericFailure_AndIncrementFailedCount()
    {
        var fixture = CreateFixture(active: true);
        var result = await fixture.Service.LoginAsync(new LoginRequest(fixture.Email, "Wrong.Passw0rd!", "127.0.0.1", "corr"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(result.User);
        Assert.Equal(1, fixture.Store.User.FailedLoginCount);
        Assert.Contains(fixture.Store.Attempts, attempt => !attempt.Succeeded && attempt.FailureReason == LoginFailureReasons.InvalidCredentials);
    }

    [Fact]
    public async Task UnknownUser_Should_ReturnGenericFailure_WithoutRevealingExistence()
    {
        var fixture = CreateFixture(active: true);
        var result = await fixture.Service.LoginAsync(new LoginRequest("missing@leaveflow.test", Password, null, "corr"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, fixture.Store.User.FailedLoginCount);
        Assert.Contains(fixture.Store.Attempts, attempt => attempt.FailureReason == LoginFailureReasons.UserNotFound);
    }

    [Fact]
    public async Task InactiveUser_Should_NotLogin()
    {
        var fixture = CreateFixture(active: false);
        var result = await fixture.Service.LoginAsync(new LoginRequest(fixture.Email, Password, null, "corr"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(fixture.Store.User.LastLoginAt);
        Assert.Contains(fixture.Store.Attempts, attempt => attempt.FailureReason == LoginFailureReasons.InactiveAccount);
    }

    [Fact]
    public async Task LockedUser_Should_NotLogin()
    {
        var fixture = CreateFixture(active: true);
        fixture.Store.User = fixture.Store.User with
        {
            FailedLoginCount = 5,
            LockoutEnd = DateTime.UtcNow.AddMinutes(15)
        };

        var result = await fixture.Service.LoginAsync(new LoginRequest(fixture.Email, Password, null, "corr"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(fixture.Store.Attempts, attempt => attempt.FailureReason == LoginFailureReasons.LockedOut);
        Assert.Null(fixture.Store.User.LastLoginAt);
    }

    [Fact]
    public async Task RepeatedFailures_Should_ApplyLockout_FromSettings()
    {
        var fixture = CreateFixture(active: true);
        for (var i = 0; i < 5; i++)
        {
            await fixture.Service.LoginAsync(new LoginRequest(fixture.Email, "Wrong.Passw0rd!", null, "corr"), CancellationToken.None);
        }

        Assert.Equal(5, fixture.Store.User.FailedLoginCount);
        Assert.NotNull(fixture.Store.User.LockoutEnd);
        Assert.Contains(fixture.Store.Audits, audit => audit.Action == AuditActions.Lockout);
    }

    [Fact]
    public async Task SuccessfulLogin_Should_ResetFailedCountAndLockout()
    {
        var fixture = CreateFixture(active: true);
        fixture.Store.User = fixture.Store.User with
        {
            FailedLoginCount = 3,
            LockoutEnd = DateTime.UtcNow.AddMinutes(-1)
        };

        var result = await fixture.Service.LoginAsync(new LoginRequest(fixture.Email, Password, null, "corr"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(0, fixture.Store.User.FailedLoginCount);
        Assert.Null(fixture.Store.User.LockoutEnd);
        Assert.NotNull(fixture.Store.User.LastLoginAt);
    }

    private Fixture CreateFixture(bool active)
    {
        var userId = Guid.NewGuid();
        var email = "consultant.one@leaveflow.test";
        var store = new InMemoryIdentityStore
        {
            User = new UserAuthRecord(
                userId,
                email,
                "Demo Consultant",
                _hasher.HashPassword(Password),
                active,
                0,
                null,
                null)
        };
        store.Roles[userId] = [RoleNames.Consultant];

        var settings = Options.Create(new AuthenticationSettings
        {
            MaxFailedAccessAttempts = 5,
            LockoutDurationMinutes = 15,
            MaxPasswordLength = 256
        });

        var service = new LoginService(
            store,
            store,
            store,
            store,
            store,
            store,
            _hasher,
            settings,
            TimeProvider.System,
            NullLogger<LoginService>.Instance);

        return new Fixture(service, store, email);
    }

    private sealed record Fixture(LoginService Service, InMemoryIdentityStore Store, string Email);

    private sealed class InMemoryIdentityStore :
        IUserAuthRepository,
        IUserRoleRepository,
        IConsultantIdentityRepository,
        IManagerIdentityRepository,
        ILoginAttemptRepository,
        IAuditLogRepository
    {
        public required UserAuthRecord User { get; set; }
        public Dictionary<Guid, IReadOnlyList<string>> Roles { get; } = new();
        public List<LoginAttemptRecord> Attempts { get; } = [];
        public List<AuditLogRecord> Audits { get; } = [];

        public Task<UserAuthRecord?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                EmailNormalizer.Normalize(User.Email) == normalizedEmail ? User : null);
        }

        public Task UpdateLoginSuccessAsync(Guid userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken = default)
        {
            User = User with { FailedLoginCount = 0, LockoutEnd = null, LastLoginAt = lastLoginAtUtc };
            return Task.CompletedTask;
        }

        public Task<FailedLoginUpdate> RecordFailedLoginAsync(
            Guid userId,
            int maxFailedAccessAttempts,
            int lockoutDurationMinutes,
            CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;
            var count = User.FailedLoginCount;
            DateTime? lockoutEnd = User.LockoutEnd;
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

            User = User with { FailedLoginCount = count, LockoutEnd = lockoutEnd };
            return Task.FromResult(new FailedLoginUpdate(count, lockoutEnd, lockoutApplied));
        }

        public Task UpdatePasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
        {
            User = User with { PasswordHash = passwordHash };
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Roles.TryGetValue(userId, out var roles) ? roles : Array.Empty<string>());
        }

        Task<Guid?> IConsultantIdentityRepository.GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Guid?>(null);
        }

        Task<Guid?> IManagerIdentityRepository.GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Guid?>(null);
        }

        public Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
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
    }
}
