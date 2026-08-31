using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.LeaveRequests;

namespace LeaveFlow.Application.Ai.Tools;

public sealed class GetMyLeaveRequestsTool(ILeaveRequestService leaveRequestService) : IAiTool
{
    public AiToolDefinition Definition { get; } = new(
        "GetMyLeaveRequests",
        "Gets the authenticated consultant user's own leave requests. Never accepts consultant identifiers.",
        AiToolSchemas.LeaveRequestSearch);

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

        var status = AiToolArgumentReader.GetString(args, "status", 20);
        if (status is not null && status is not ("Pending" or "Approved" or "Rejected"))
        {
            return AiToolExecutionResult.Failure("InvalidArguments");
        }

        if (!AiToolArgumentReader.IsValidRange(startDate, endDate, 366))
        {
            return AiToolExecutionResult.Failure("InvalidDateRange");
        }

        var result = await leaveRequestService.GetMineAsync(
            context.UserId,
            new LeaveRequestSearchRequest(
                status,
                startDate,
                endDate,
                1,
                20),
            cancellationToken);

        return result is null
            ? AiToolExecutionResult.Failure("Unauthorized")
            : AiToolExecutionResult.Success(new
            {
                result.TotalCount,
                Requests = result.Items.Select(item => new
                {
                    item.Id,
                    item.Reason,
                    item.StartDate,
                    item.EndDate,
                    item.Status,
                    item.CreatedAt
                })
            });
    }
}
