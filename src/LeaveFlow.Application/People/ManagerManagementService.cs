using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Common;

namespace LeaveFlow.Application.People;

public sealed class ManagerManagementService(IManagerManagementRepository repository) : IManagerManagementService
{
    public Task<PagedResult<ManagerListItem>> SearchAsync(
        PeopleSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = request with
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };

        return repository.SearchAsync(normalized, cancellationToken);
    }

    public ValidationResult Validate(ManagerInput input)
    {
        return PeopleValidation.ValidateManager(input);
    }
}
