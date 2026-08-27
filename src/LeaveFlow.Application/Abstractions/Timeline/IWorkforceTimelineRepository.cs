using LeaveFlow.Application.Common;
using LeaveFlow.Application.Timeline;

namespace LeaveFlow.Application.Abstractions.Timeline;

public interface IWorkforceTimelineRepository
{
    Task<PagedResult<WorkforceTimelineDataRow>> GetForAdminAsync(
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default);

    Task<PagedResult<WorkforceTimelineDataRow>> GetForManagerAsync(
        Guid managerId,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default);
}
