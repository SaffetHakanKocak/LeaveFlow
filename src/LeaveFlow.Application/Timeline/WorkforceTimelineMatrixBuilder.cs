namespace LeaveFlow.Application.Timeline;

public static class WorkforceTimelineMatrixBuilder
{
    public static IReadOnlyList<TimelineRow> Build(
        IReadOnlyList<WorkforceTimelineDataRow> dataRows,
        IReadOnlyList<TimelineDay> days)
    {
        return dataRows
            .GroupBy(row => new
            {
                row.ConsultantId,
                row.ConsultantName,
                row.ConsultantEmail,
                row.ConsultantIsActive
            })
            .OrderBy(group => group.Key.ConsultantName)
            .Select(group =>
            {
                var leaveByDate = group
                    .Where(row => row.LeaveDate is not null)
                    .GroupBy(row => row.LeaveDate!.Value)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.First());

                var cells = days
                    .Select(day =>
                    {
                        leaveByDate.TryGetValue(day.Date, out var leave);
                        return new TimelineCell(
                            day.Date,
                            day.IsWeekend,
                            day.IsToday,
                            leave is not null,
                            leave?.LeaveRequestId,
                            leave?.Reason);
                    })
                    .ToArray();

                return new TimelineRow(
                    group.Key.ConsultantId,
                    group.Key.ConsultantName,
                    group.Key.ConsultantEmail,
                    group.Key.ConsultantIsActive,
                    cells);
            })
            .ToArray();
    }
}
