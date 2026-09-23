namespace LeaveFlow.Application.Ai;

public sealed record AiAssistantInput(Guid UserId, string UserDisplayName, string Prompt);

public sealed record AiChatRequest(
    Guid UserId,
    string UserDisplayName,
    string Prompt,
    IReadOnlyList<AiToolDefinition>? Tools = null,
    IReadOnlyList<AiToolResultMessage>? ToolResults = null);

public sealed record AiChatResponse(
    string Message,
    IReadOnlyList<AiToolCall>? ToolCalls = null);

public sealed record AiToolDefinition(
    string Name,
    string Description,
    string ParametersJsonSchema);

public sealed record AiToolCall(
    string Id,
    string Name,
    string ArgumentsJson);

public sealed record AiToolResultMessage(
    string ToolCallId,
    string ToolName,
    string ResultJson,
    bool Succeeded);

public sealed record AiAssistantResult(
    bool Succeeded,
    bool IsEnabled,
    string? Message,
    string? ErrorMessage,
    IReadOnlyList<string>? UsedTools = null)
{
    public static AiAssistantResult Disabled() =>
        new(false, false, null, "AI Asistan şu anda kapalı.", []);

    public static AiAssistantResult Success(string message, IReadOnlyList<string>? usedTools = null) =>
        new(true, true, message, null, usedTools ?? []);

    public static AiAssistantResult Failure(string errorMessage) =>
        new(false, true, null, errorMessage, []);
}
