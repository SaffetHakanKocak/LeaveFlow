using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Timeline;

namespace LeaveFlow.Application.Ai.Tools;

public sealed class GetTeamAvailabilityTool(IWorkforceTimelineService timelineService) : IAiTool
{
    public AiToolDefinition Definition { get; } = new(
        "GetTeamAvailability",
        "Gets a manager/admin scoped team availability timeline. Consultant users are not authorized for team scope.",
        AiToolSchemas.DateRange);

    public async Task<AiToolExecutionResult> ExecuteAsync(
        AiToolExecutionContext context,
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        if (!AiToolArgumentReader.TryParse(argumentsJson, out var args))
        {
            return AiToolExecutionResult.Failure("InvalidArguments");
        }

        if (!AiToolArgumentReader.TryGetDate(args, "startDate", out var startDate)
            || !AiToolArgumentReader.TryGetDate(args, "endDate", out var endDate))
        {
            return AiToolExecutionResult.Failure("InvalidArguments");
        }

        if (!AiToolArgumentReader.IsValidRange(startDate, endDate, WorkforceTimelineSettings.MaxRangeDays))
        {
            return AiToolExecutionResult.Failure("InvalidDateRange");
        }

        var timeline = await timelineService.GetAsync(
            context.UserId,
            new WorkforceTimelineQuery(startDate, endDate, null, null, false, 1, 25),
            cancellationToken);

        return timeline is null
            ? AiToolExecutionResult.Failure("Unauthorized")
            : AiToolExecutionResult.Success(new
            {
                timeline.Query.StartDate,
                timeline.Query.EndDate,
                timeline.TotalCount,
                Consultants = timeline.Rows.Select(row => new
                {
                    row.ConsultantName,
                    LeaveDays = row.Cells
                        .Where(cell => cell.IsOnLeave)
                        .Select(cell => new { cell.Date, cell.Reason })
                })
            });
    }
}
