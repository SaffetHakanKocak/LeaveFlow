using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.Holidays;

public interface IOfficialHolidayService
{
    Task<PagedResult<OfficialHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default);

    ValidationResult Validate(OfficialHolidayInput input);
}
