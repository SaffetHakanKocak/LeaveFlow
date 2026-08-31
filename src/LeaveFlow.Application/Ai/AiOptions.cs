namespace LeaveFlow.Application.Ai;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public bool Enabled { get; set; }

    public string Provider { get; set; } = "Azure";

    public int MaxPromptLength { get; set; } = 2000;

    public int TimeoutSeconds { get; set; } = 15;

    public AzureAiOptions Azure { get; set; } = new();
}

public sealed class AzureAiOptions
{
    public string? Endpoint { get; set; }

    public string? Deployment { get; set; }

    public string? ApiKey { get; set; }

    public string ApiVersion { get; set; } = "2024-02-15-preview";
}
