using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.People;

public interface IManagerManagementService
{
    Task<PagedResult<ManagerListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default);

    ValidationResult Validate(ManagerInput input);
}
