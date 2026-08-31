using LeaveFlow.Application.Abstractions.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Application.Ai;

public sealed class AiAssistantService(
    IAiChatClient chatClient,
    IOptions<AiOptions> options,
    ILogger<AiAssistantService> logger) : IAiAssistantService
{
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
            return AiAssistantResult.Failure("Kullanıcı oturumu doğrulanamadı.");
        }

        if (string.IsNullOrWhiteSpace(input.Prompt))
        {
            return AiAssistantResult.Failure("Mesaj alanı zorunludur.");
        }

        if (input.Prompt.Length > settings.MaxPromptLength)
        {
            return AiAssistantResult.Failure($"Mesaj en fazla {settings.MaxPromptLength} karakter olabilir.");
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 1, 60)));

            var response = await chatClient.SendAsync(
                new AiChatRequest(input.UserId, input.UserDisplayName, input.Prompt.Trim()),
                timeout.Token);

            return string.IsNullOrWhiteSpace(response.Message)
                ? AiAssistantResult.Failure("AI Asistan boş yanıt döndürdü.")
                : AiAssistantResult.Success(response.Message.Trim());
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("AI assistant request timed out for user {UserId}.", input.UserId);
            return AiAssistantResult.Failure("AI Asistan zaman aşımına uğradı. Lütfen tekrar deneyin.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "AI assistant request failed for user {UserId}.", input.UserId);
            return AiAssistantResult.Failure("AI Asistan şu anda yanıt veremiyor. Lütfen daha sonra tekrar deneyin.");
        }
    }
}
