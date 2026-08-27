using LeaveFlow.Application.Timeline;

namespace LeaveFlow.Web.Models;

public sealed class WorkforceTimelineViewModel
{
    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public Guid? ManagerId { get; init; }

    public string? ConsultantSearch { get; init; }

    public bool IncludeInactive { get; init; }

    public WorkforceTimelineResult? Timeline { get; init; }
}
