using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;

namespace LeaveFlow.UnitTests.Holidays;

internal sealed class StubOrganizationHolidayRepository : IOrganizationHolidayRepository
{
    public Task<PagedResult<OrganizationHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PagedResult<OrganizationHolidayListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<OrganizationHolidayDetail?> GetByIdAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<OrganizationHolidayDetail?>(null);
    }

    public Task<Guid> CreateAsync(OrganizationHolidayInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<bool> UpdateAsync(Guid holidayId, OrganizationHolidayInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> SetActiveAsync(Guid holidayId, bool isActive, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}

internal sealed class StubOfficialHolidayRepository : IOfficialHolidayRepository
{
    public Task<PagedResult<OfficialHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PagedResult<OfficialHolidayListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<OfficialHolidayDetail?> GetByIdAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<OfficialHolidayDetail?>(null);
    }

    public Task<Guid> CreateAsync(OfficialHolidayInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task<bool> UpdateAsync(Guid holidayId, OfficialHolidayInput input, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<bool> DeleteAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
