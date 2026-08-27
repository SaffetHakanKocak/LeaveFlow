using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.People;

public interface IConsultantManagementRepository
{
    Task<PagedResult<ConsultantListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default);

    Task<ConsultantDetail?> GetByIdAsync(Guid consultantId, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(ConsultantInput input, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid consultantId, ConsultantInput input, CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(Guid consultantId, bool isActive, CancellationToken cancellationToken = default);
}
