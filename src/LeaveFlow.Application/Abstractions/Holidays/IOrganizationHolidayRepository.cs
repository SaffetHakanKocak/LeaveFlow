using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;

namespace LeaveFlow.Application.Abstractions.Holidays;

public interface IOrganizationHolidayRepository
{
    Task<PagedResult<OrganizationHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default);

    Task<OrganizationHolidayDetail?> GetByIdAsync(Guid holidayId, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(OrganizationHolidayInput input, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid holidayId, OrganizationHolidayInput input, CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(Guid holidayId, bool isActive, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid holidayId, CancellationToken cancellationToken = default);
}
