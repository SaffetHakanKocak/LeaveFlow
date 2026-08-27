using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.Holidays;

public interface IOrganizationHolidayService
{
    Task<PagedResult<OrganizationHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default);

    ValidationResult Validate(OrganizationHolidayInput input);
}
