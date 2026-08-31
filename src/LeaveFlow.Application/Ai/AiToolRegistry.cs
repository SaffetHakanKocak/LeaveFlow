using LeaveFlow.Application.Abstractions.Ai;

namespace LeaveFlow.Application.Ai;

public sealed class AiToolRegistry(IEnumerable<IAiTool> tools) : IAiToolRegistry
{
    private readonly Dictionary<string, IAiTool> _tools = tools.ToDictionary(
        tool => tool.Definition.Name,
        StringComparer.Ordinal);

    public IReadOnlyList<AiToolDefinition> GetDefinitions()
    {
        return _tools.Values.Select(tool => tool.Definition).ToArray();
    }

    public bool TryGet(string name, out IAiTool tool)
    {
        return _tools.TryGetValue(name, out tool!);
    }
}
