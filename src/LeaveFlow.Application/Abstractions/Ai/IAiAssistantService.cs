using LeaveFlow.Application.Ai;

namespace LeaveFlow.Application.Abstractions.Ai;

public interface IAiAssistantService
{
    Task<AiAssistantResult> SendAsync(AiAssistantInput input, CancellationToken cancellationToken = default);
}
