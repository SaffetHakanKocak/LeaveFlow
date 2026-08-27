using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.People;

public interface IManagerManagementRepository
{
    Task<PagedResult<ManagerListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default);

    Task<ManagerDetail?> GetByIdAsync(Guid managerId, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ManagerInput input, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid managerId, ManagerInput input, CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(Guid managerId, bool isActive, CancellationToken cancellationToken = default);
}
