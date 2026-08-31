namespace LeaveFlow.Application.Ai;

public sealed record AiAssistantInput(Guid UserId, string UserDisplayName, string Prompt);

public sealed record AiChatRequest(Guid UserId, string UserDisplayName, string Prompt);

public sealed record AiChatResponse(string Message);

public sealed record AiAssistantResult(
    bool Succeeded,
    bool IsEnabled,
    string? Message,
    string? ErrorMessage)
{
    public static AiAssistantResult Disabled() =>
        new(false, false, null, "AI Asistan şu anda kapalı.");

    public static AiAssistantResult Success(string message) =>
        new(true, true, message, null);

    public static AiAssistantResult Failure(string errorMessage) =>
        new(false, true, null, errorMessage);
}
