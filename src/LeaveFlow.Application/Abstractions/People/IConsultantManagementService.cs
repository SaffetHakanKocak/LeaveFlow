using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.People;

public interface IConsultantManagementService
{
    Task<PagedResult<ConsultantListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default);

    Task<ConsultantDetail?> GetForActorAsync(Guid actorUserId, Guid consultantId, CancellationToken cancellationToken = default);

    ValidationResult Validate(ConsultantInput input);
}
