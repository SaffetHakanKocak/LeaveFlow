namespace LeaveFlow.Application.Ai;

public sealed record AiToolExecutionContext(Guid UserId, string UserDisplayName);

public sealed record AiToolExecutionResult(bool Succeeded, object? Data, string? ErrorCode)
{
    public static AiToolExecutionResult Success(object data) => new(true, data, null);

    public static AiToolExecutionResult Failure(string errorCode) => new(false, null, errorCode);
}

public sealed record AiToolAuditMetadata(
    string ToolName,
    Guid UserId,
    DateTime Timestamp,
    bool Succeeded,
    long DurationMilliseconds,
    string? ErrorCode);
