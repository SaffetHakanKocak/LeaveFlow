using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Reporting;

namespace LeaveFlow.Application.Ai.Tools;

public sealed class GetOrganizationLeaveStatisticsTool(IReportingService reportingService) : IAiTool
{
    public AiToolDefinition Definition { get; } = new(
        "GetOrganizationLeaveStatistics",
        "Gets manager/admin scoped leave statistics. Consultant users are not authorized for organization statistics.",
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

        return reports is null
            ? AiToolExecutionResult.Failure("Unauthorized")
            : AiToolExecutionResult.Success(new
            {
                reports.Query,
                reports.SummaryMetrics,
                reports.ConsultantLeaveUsage,
                reports.MonthlyLeaveActivity,
                reports.TeamLeaveUsage,
                reports.PeakLeaveDays,
                reports.StatusDistribution
            });
    }
}
