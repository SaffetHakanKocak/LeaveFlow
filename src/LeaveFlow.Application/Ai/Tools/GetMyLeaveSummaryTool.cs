using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.Reporting;

namespace LeaveFlow.Application.Ai.Tools;

public sealed class GetMyLeaveSummaryTool(IReportingService reportingService) : IAiTool
{
    public AiToolDefinition Definition { get; } = new(
        "GetMyLeaveSummary",
        "Gets a role-scoped leave summary for the authenticated user.",
        AiToolSchemas.EmptyObject);

    public async Task<AiToolExecutionResult> ExecuteAsync(
        AiToolExecutionContext context,
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        if (!AiToolArgumentReader.TryParse(argumentsJson, out _))
        {
            return AiToolExecutionResult.Failure("InvalidArguments");
        }

        var dashboard = await reportingService.GetDashboardAsync(context.UserId, cancellationToken);
        return dashboard is null
            ? AiToolExecutionResult.Failure("Unauthorized")
            : AiToolExecutionResult.Success(new
            {
                dashboard.Scope,
                dashboard.Metrics,
                RecentLeaveRequests = dashboard.RecentLeaveRequests
            });
    }
}
