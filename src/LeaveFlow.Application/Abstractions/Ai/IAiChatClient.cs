using LeaveFlow.Application.Ai;

namespace LeaveFlow.Application.Abstractions.Ai;

public interface IAiChatClient
{
    Task<AiChatResponse> SendAsync(AiChatRequest request, CancellationToken cancellationToken = default);
}
