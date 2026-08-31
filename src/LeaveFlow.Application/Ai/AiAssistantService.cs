using System.Diagnostics;
using System.Text.Json;
using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Application.Ai;

public sealed class AiAssistantService(
    IAiChatClient chatClient,
    IAiToolRegistry toolRegistry,
    IAuditLogRepository auditLogRepository,
    IOptions<AiOptions> options,
    ILogger<AiAssistantService> logger) : IAiAssistantService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int MaxToolRounds = 3;

    public async Task<AiAssistantResult> SendAsync(AiAssistantInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var settings = options.Value;
        if (!settings.Enabled)
        {
            return AiAssistantResult.Disabled();
        }

        if (input.UserId == Guid.Empty)
        {
            return AiAssistantResult.Failure("Kullanici oturumu dogrulanamadi.");
        }

        if (string.IsNullOrWhiteSpace(input.Prompt))
        {
            return AiAssistantResult.Failure("Mesaj alani zorunludur.");
        }

        if (input.Prompt.Length > settings.MaxPromptLength)
        {
            return AiAssistantResult.Failure($"Mesaj en fazla {settings.MaxPromptLength} karakter olabilir.");
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 1, 60)));

            var tools = toolRegistry.GetDefinitions();
            var toolResults = new List<AiToolResultMessage>();
            var usedTools = new List<string>();
            AiChatResponse response = new(string.Empty);

            for (var round = 0; round < MaxToolRounds; round++)
            {
                response = await chatClient.SendAsync(
                    new AiChatRequest(
                        input.UserId,
                        input.UserDisplayName,
                        input.Prompt.Trim(),
                        tools,
                        toolResults),
                    timeout.Token);

                if (response.ToolCalls is null || response.ToolCalls.Count == 0)
                {
                    break;
                }

                foreach (var toolCall in response.ToolCalls.Take(5))
                {
                    var result = await ExecuteToolCallAsync(
                        new AiToolExecutionContext(input.UserId, input.UserDisplayName),
                        toolCall,
                        timeout.Token);

                    usedTools.Add(toolCall.Name);
                    toolResults.Add(new AiToolResultMessage(
                        toolCall.Id,
                        toolCall.Name,
                        JsonSerializer.Serialize(result, JsonOptions),
                        result.Succeeded));
                }
            }

            return string.IsNullOrWhiteSpace(response.Message)
                ? AiAssistantResult.Failure("AI Asistan bos yanit dondurdu.")
                : AiAssistantResult.Success(response.Message.Trim(), usedTools.Distinct(StringComparer.Ordinal).ToArray());
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("AI assistant request timed out for user {UserId}.", input.UserId);
            return AiAssistantResult.Failure("AI Asistan zaman asimina ugradi. Lutfen tekrar deneyin.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "AI assistant request failed for user {UserId}.", input.UserId);
            return AiAssistantResult.Failure("AI Asistan su anda yanit veremiyor. Lutfen daha sonra tekrar deneyin.");
        }
    }

    private async Task<AiToolExecutionResult> ExecuteToolCallAsync(
        AiToolExecutionContext context,
        AiToolCall toolCall,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var succeeded = false;
        string? errorCode = null;

        try
        {
            if (!toolRegistry.TryGet(toolCall.Name, out var tool))
            {
                errorCode = "UnknownTool";
                return AiToolExecutionResult.Failure(errorCode);
            }

            var result = await tool.ExecuteAsync(context, toolCall.ArgumentsJson, cancellationToken);
            succeeded = result.Succeeded;
            errorCode = result.ErrorCode;
            return result;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "AI tool call failed for tool {ToolName} and user {UserId}.", toolCall.Name, context.UserId);
            errorCode = "ToolFailure";
            return AiToolExecutionResult.Failure(errorCode);
        }
        finally
        {
            var duration = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            await AuditToolCallAsync(context.UserId, toolCall.Name, succeeded, duration, errorCode, cancellationToken);
        }
    }

    private async Task AuditToolCallAsync(
        Guid userId,
        string toolName,
        bool succeeded,
        long durationMilliseconds,
        string? errorCode,
        CancellationToken cancellationToken)
    {
        var metadata = new AiToolAuditMetadata(
            toolName,
            userId,
            DateTime.UtcNow,
            succeeded,
            durationMilliseconds,
            errorCode);

        await auditLogRepository.InsertAsync(
            new AuditLogRecord(
                userId,
                AuditActions.AiToolCall,
                "AiTool",
                toolName,
                succeeded ? AuditOutcomes.Success : AuditOutcomes.Failure,
                null,
                JsonSerializer.Serialize(metadata, JsonOptions)),
            cancellationToken);
    }
}
