using LeaveFlow.Application.Abstractions.Authorization;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;

namespace LeaveFlow.UnitTests.People;

internal sealed class StubConsultantRepository : IConsultantManagementRepository
{
    public Task<PagedResult<ConsultantListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PagedResult<ConsultantListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<ConsultantDetail?> GetByIdAsync(Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ConsultantDetail?>(null);
    }

    public Task<Guid> CreateAsync(ConsultantInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<bool> UpdateAsync(Guid consultantId, ConsultantInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SetActiveAsync(Guid consultantId, bool isActive, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

internal sealed class StubManagerRepository : IManagerManagementRepository
{
    public Task<PagedResult<ManagerListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PagedResult<ManagerListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<ManagerDetail?> GetByIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ManagerDetail?>(null);
    }

    public Task<Guid> CreateAsync(ManagerInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<bool> UpdateAsync(Guid managerId, ManagerInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SetActiveAsync(Guid managerId, bool isActive, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

internal sealed class StubConsultantAuthorizationService : IConsultantResourceAuthorizationService
{
    public Task<bool> CanAccessConsultantAsync(Guid actorUserId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
