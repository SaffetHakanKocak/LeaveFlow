using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;

namespace LeaveFlow.SecurityTests.Fakes;

public sealed class InMemoryHolidayStore : IOrganizationHolidayRepository, IOfficialHolidayRepository
{
    private readonly Dictionary<Guid, OrganizationHolidayDetail> _organizationHolidays = new();
    private readonly Dictionary<Guid, OfficialHolidayDetail> _officialHolidays = new();

    public Task<PagedResult<OrganizationHolidayListItem>> SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken = default)
    {
        var items = _organizationHolidays.Values
            .Where(item => request.IsActive is null || item.IsActive == request.IsActive)
            .Where(item => string.IsNullOrWhiteSpace(request.Search) || item.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
            .Where(item => request.FromDate is null || item.EndDate >= request.FromDate)
            .Where(item => request.ToDate is null || item.StartDate <= request.ToDate)
            .OrderByDescending(item => item.StartDate)
            .ToArray();

        var page = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new OrganizationHolidayListItem(
                item.Id,
                item.Name,
                item.StartDate,
                item.EndDate,
                item.IsActive,
                item.Days.Count,
                items.Length))
            .ToArray();

        return Task.FromResult(new PagedResult<OrganizationHolidayListItem>(page, request.PageNumber, request.PageSize, items.Length));
    }

    public Task<OrganizationHolidayDetail?> GetByIdAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_organizationHolidays.TryGetValue(holidayId, out var holiday) ? holiday : null);
    }

    public Task<Guid> CreateAsync(OrganizationHolidayInput input, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var startDate = input.StartDate.GetValueOrDefault();
        var endDate = input.EndDate.GetValueOrDefault();
        _organizationHolidays[id] = new OrganizationHolidayDetail(
            id,
            input.Name,
            startDate,
            endDate,
            input.IsActive,
            HolidayDateRange.GenerateInclusive(startDate, endDate));

        return Task.FromResult(id);
    }

    public Task<bool> UpdateAsync(Guid holidayId, OrganizationHolidayInput input, CancellationToken cancellationToken = default)
    {
        if (!_organizationHolidays.ContainsKey(holidayId))
        {
            return Task.FromResult(false);
        }

        var startDate = input.StartDate.GetValueOrDefault();
        var endDate = input.EndDate.GetValueOrDefault();
        _organizationHolidays[holidayId] = new OrganizationHolidayDetail(
            holidayId,
            input.Name,
            startDate,
            endDate,
            input.IsActive,
            HolidayDateRange.GenerateInclusive(startDate, endDate));

        return Task.FromResult(true);
    }

    public Task<bool> SetActiveAsync(Guid holidayId, bool isActive, CancellationToken cancellationToken = default)
    {
        if (!_organizationHolidays.TryGetValue(holidayId, out var holiday))
        {
            return Task.FromResult(false);
        }

        _organizationHolidays[holidayId] = holiday with { IsActive = isActive };
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_organizationHolidays.Remove(holidayId));
    }

    Task<PagedResult<OfficialHolidayListItem>> IOfficialHolidayRepository.SearchAsync(HolidaySearchRequest request, CancellationToken cancellationToken)
    {
        var items = _officialHolidays.Values
            .Where(item => string.IsNullOrWhiteSpace(request.Search) || item.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
            .Where(item => request.FromDate is null || item.EndDate >= request.FromDate)
            .Where(item => request.ToDate is null || item.StartDate <= request.ToDate)
            .OrderByDescending(item => item.StartDate)
            .ToArray();

        var page = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new OfficialHolidayListItem(
                item.Id,
                item.Name,
                item.StartDate,
                item.EndDate,
                item.Days.Count,
                items.Length))
            .ToArray();

        return Task.FromResult(new PagedResult<OfficialHolidayListItem>(page, request.PageNumber, request.PageSize, items.Length));
    }

    Task<OfficialHolidayDetail?> IOfficialHolidayRepository.GetByIdAsync(Guid holidayId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_officialHolidays.TryGetValue(holidayId, out var holiday) ? holiday : null);
    }

    Task<Guid> IOfficialHolidayRepository.CreateAsync(OfficialHolidayInput input, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var startDate = input.StartDate.GetValueOrDefault();
        var endDate = input.EndDate.GetValueOrDefault();
        _officialHolidays[id] = new OfficialHolidayDetail(
            id,
            input.Name,
            startDate,
            endDate,
            HolidayDateRange.GenerateInclusive(startDate, endDate));

        return Task.FromResult(id);
    }

    Task<bool> IOfficialHolidayRepository.UpdateAsync(Guid holidayId, OfficialHolidayInput input, CancellationToken cancellationToken)
    {
        if (!_officialHolidays.ContainsKey(holidayId))
        {
            return Task.FromResult(false);
        }

        var startDate = input.StartDate.GetValueOrDefault();
        var endDate = input.EndDate.GetValueOrDefault();
        _officialHolidays[holidayId] = new OfficialHolidayDetail(
            holidayId,
            input.Name,
            startDate,
            endDate,
            HolidayDateRange.GenerateInclusive(startDate, endDate));

        return Task.FromResult(true);
    }

    Task<bool> IOfficialHolidayRepository.DeleteAsync(Guid holidayId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_officialHolidays.Remove(holidayId));
    }
}
