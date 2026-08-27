using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.UnitTests.Authorization;

public sealed class ConsultantResourceAuthorizationServiceTests
{
    [Fact]
    public async Task Consultant_Should_AccessOwnResource_ButNotAnotherConsultant()
    {
        var ownId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var service = CreateService(
            userId,
            [RoleNames.Consultant],
            consultantId: ownId,
            managerId: null,
            assignedConsultantIds: []);

        Assert.True(await service.CanAccessConsultantAsync(userId, ownId));
        Assert.False(await service.CanAccessConsultantAsync(userId, otherId));
    }

    [Fact]
    public async Task Manager_Should_AccessAssignedConsultant_ButNotAnotherManagersConsultant()
    {
        var assignedId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var service = CreateService(
            userId,
            [RoleNames.Manager],
            consultantId: null,
            managerId,
            assignedConsultantIds: [assignedId]);

        Assert.True(await service.CanAccessConsultantAsync(userId, assignedId));
        Assert.False(await service.CanAccessConsultantAsync(userId, otherId));
    }

    [Fact]
    public async Task Administrator_Should_AccessAnyConsultant()
    {
        var consultantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var service = CreateService(
            userId,
            [RoleNames.Administrator],
            consultantId: null,
            managerId: null,
            assignedConsultantIds: []);

        Assert.True(await service.CanAccessConsultantAsync(userId, consultantId));
    }

    private static ConsultantResourceAuthorizationService CreateService(
        Guid userId,
        IReadOnlyList<string> roles,
        Guid? consultantId,
        Guid? managerId,
        IReadOnlyList<Guid> assignedConsultantIds)
    {
        return new ConsultantResourceAuthorizationService(
            new StubUserRoleRepository(userId, roles),
            new StubConsultantIdentityRepository(userId, consultantId),
            new StubManagerIdentityRepository(userId, managerId, assignedConsultantIds));
    }

    private sealed class StubUserRoleRepository(Guid userId, IReadOnlyList<string> roles) : IUserRoleRepository
    {
        public Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid requestedUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(requestedUserId == userId ? roles : Array.Empty<string>());
        }
    }

    private sealed class StubConsultantIdentityRepository(Guid userId, Guid? consultantId) : IConsultantIdentityRepository
    {
        public Task<Guid?> GetIdByUserIdAsync(Guid requestedUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(requestedUserId == userId ? consultantId : null);
        }
    }

    private sealed class StubManagerIdentityRepository(
        Guid userId,
        Guid? managerId,
        IReadOnlyList<Guid> assignedConsultantIds) : IManagerIdentityRepository
    {
        public Task<Guid?> GetIdByUserIdAsync(Guid requestedUserId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(requestedUserId == userId ? managerId : null);
        }

        public Task<bool> IsAssignedToConsultantAsync(Guid requestedManagerId, Guid consultantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(managerId == requestedManagerId && assignedConsultantIds.Contains(consultantId));
        }
    }
}
