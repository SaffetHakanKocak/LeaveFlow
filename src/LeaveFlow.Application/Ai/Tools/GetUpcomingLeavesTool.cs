using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Reporting;

namespace LeaveFlow.Application.Ai.Tools;

public sealed class GetUpcomingLeavesTool(IReportingService reportingService) : IAiTool
{
    public AiToolDefinition Definition { get; } = new(
        "GetUpcomingLeaves",
        "Gets upcoming approved leaves scoped to the authenticated user's role.",
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

        if (!AiToolArgumentReader.IsValidRange(startDate, endDate, 366))
        {
            return AiToolExecutionResult.Failure("InvalidDateRange");
        }

        var reports = await reportingService.GetReportsAsync(
            context.UserId,
            new ReportQuery(startDate, endDate, null, null),
            cancellationToken);
        if (reports is not null)
        {
            return AiToolExecutionResult.Success(new { reports.Query, Leaves = reports.UpcomingLeaves });
        }

        var dashboard = await reportingService.GetDashboardAsync(context.UserId, cancellationToken);
        return dashboard is null
            ? AiToolExecutionResult.Failure("Unauthorized")
            : AiToolExecutionResult.Success(new { Leaves = dashboard.UpcomingLeaves });
    }
}
