using LeaveFlow.Application.Ai;

namespace LeaveFlow.Application.Abstractions.Ai;

public interface IAiToolRegistry
{
    IReadOnlyList<AiToolDefinition> GetDefinitions();

    bool TryGet(string name, out IAiTool tool);
}
