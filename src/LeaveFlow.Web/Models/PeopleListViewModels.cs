using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;

namespace LeaveFlow.Web.Models;

public sealed class ConsultantListViewModel
{
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    public PagedResult<ConsultantListItem> Results { get; init; } = new([], 1, 25, 0);
}

public sealed class ManagerListViewModel
{
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    public PagedResult<ManagerListItem> Results { get; init; } = new([], 1, 25, 0);
}
