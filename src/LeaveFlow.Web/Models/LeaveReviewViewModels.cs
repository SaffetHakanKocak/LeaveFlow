using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;

namespace LeaveFlow.Web.Models;

public sealed class PendingLeaveRequestsViewModel
{
    public Guid? ConsultantId { get; init; }

    public string? Status { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PagedResult<PendingLeaveRequestListItem> Results { get; init; } = new([], 1, 25, 0);
}

public sealed class LeaveReviewDetailViewModel
{
    public LeaveRequestReviewDetail Request { get; init; } = default!;

    public IReadOnlyList<LeaveConflict> Conflicts { get; init; } = [];

    public ReviewDecisionViewModel Decision { get; init; } = new();

    public IReadOnlyList<DateOnly> PreviewDates =>
        Enumerable
            .Range(0, Request.CalendarDayCount)
            .Select(offset => Request.StartDate.AddDays(offset))
            .ToArray();

    public IReadOnlyList<ConflictTimelineRow> TimelineRows =>
        Conflicts
            .GroupBy(conflict => new { conflict.ConsultantId, conflict.ConsultantName, conflict.ConsultantEmail })
            .Select(group => new ConflictTimelineRow(
                group.Key.ConsultantName,
                group.Key.ConsultantEmail,
                group.Select(item => item.ConflictDate).ToHashSet()))
            .ToArray();
}

public sealed class ReviewDecisionViewModel
{
    public string? ReviewNote { get; init; }
}

public sealed record ConflictTimelineRow(
    string ConsultantName,
    string ConsultantEmail,
    IReadOnlySet<DateOnly> ConflictDates);
