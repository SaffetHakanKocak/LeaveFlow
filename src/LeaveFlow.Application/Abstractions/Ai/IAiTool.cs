using LeaveFlow.Application.Ai;

namespace LeaveFlow.Application.Abstractions.Ai;

public interface IAiTool
{
    AiToolDefinition Definition { get; }

    Task<AiToolExecutionResult> ExecuteAsync(
        AiToolExecutionContext context,
        string argumentsJson,
        CancellationToken cancellationToken = default);
}
