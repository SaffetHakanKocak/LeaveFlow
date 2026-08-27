using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;

namespace LeaveFlow.Application.Abstractions.Holidays;

public interface IOfficialHolidayRepository
{
    Task<PagedResult<OfficialHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default);

    Task<OfficialHolidayDetail?> GetByIdAsync(Guid holidayId, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(OfficialHolidayInput input, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Guid holidayId, OfficialHolidayInput input, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid holidayId, CancellationToken cancellationToken = default);
}
