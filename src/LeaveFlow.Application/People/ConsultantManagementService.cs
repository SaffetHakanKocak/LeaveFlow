using LeaveFlow.Application.Abstractions.Authorization;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Common;

namespace LeaveFlow.Application.People;

public sealed class ConsultantManagementService(
    IConsultantManagementRepository repository,
    IConsultantResourceAuthorizationService authorizationService) : IConsultantManagementService
{
    public Task<PagedResult<ConsultantListItem>> SearchAsync(
        PeopleSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return repository.SearchAsync(Normalize(request), cancellationToken);
    }

    public async Task<ConsultantDetail?> GetForActorAsync(
        Guid actorUserId,
        Guid consultantId,
        CancellationToken cancellationToken = default)
    {
        if (!await authorizationService.CanAccessConsultantAsync(actorUserId, consultantId, cancellationToken))
        {
            return null;
        }

        return await repository.GetByIdAsync(consultantId, cancellationToken);
    }

    public ValidationResult Validate(ConsultantInput input)
    {
        return PeopleValidation.ValidateConsultant(input);
    }

    private static PeopleSearchRequest Normalize(PeopleSearchRequest request)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return request with
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
