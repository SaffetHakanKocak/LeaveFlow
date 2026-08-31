using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.LeaveRequests;

namespace LeaveFlow.Application.Ai.Tools;

public sealed class GetLeaveConflictsTool(ILeaveReviewService leaveReviewService) : IAiTool
{
    public AiToolDefinition Definition { get; } = new(
        "GetLeaveConflicts",
        "Gets conflicts for a reviewable leave request using the authenticated reviewer scope.",
        AiToolSchemas.LeaveConflict);

    public async Task<AiToolExecutionResult> ExecuteAsync(
        AiToolExecutionContext context,
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        if (!AiToolArgumentReader.TryParse(argumentsJson, out var args))
        {
            return AiToolExecutionResult.Failure("InvalidArguments");
        }

        var leaveRequestId = AiToolArgumentReader.GetGuid(args, "leaveRequestId");
        if (leaveRequestId is null || leaveRequestId == Guid.Empty)
        {
            return AiToolExecutionResult.Failure("InvalidArguments");
        }

        var conflicts = await leaveReviewService.GetConflictsAsync(context.UserId, leaveRequestId.Value, cancellationToken);
        return conflicts is null
            ? AiToolExecutionResult.Failure("Unauthorized")
            : AiToolExecutionResult.Success(new { Conflicts = conflicts });
    }
}
