using LeaveFlow.Application.People;
using LeaveFlow.Application.Timeline;

namespace LeaveFlow.Application.Abstractions.Timeline;

public interface IWorkforceTimelineService
{
    Task<WorkforceTimelineResult?> GetAsync(
        Guid actorUserId,
        WorkforceTimelineQuery query,
        CancellationToken cancellationToken = default);

    ValidationResult Validate(WorkforceTimelineQuery query);
}
