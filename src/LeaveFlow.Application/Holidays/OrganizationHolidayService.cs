using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Holidays;

public sealed class OrganizationHolidayService(IOrganizationHolidayRepository repository) : IOrganizationHolidayService
{
    public Task<PagedResult<OrganizationHolidayListItem>> SearchAsync(
        HolidaySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return repository.SearchAsync(Normalize(request), cancellationToken);
    }

    public ValidationResult Validate(OrganizationHolidayInput input)
    {
        return HolidayValidation.ValidateOrganization(input);
    }

    private static HolidaySearchRequest Normalize(HolidaySearchRequest request)
    {
        return request with
        {
            Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim(),
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
    }
}
