namespace LeaveFlow.Application.Ai;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public bool Enabled { get; set; }

    public string Provider { get; set; } = "Azure";

    public int MaxPromptLength { get; set; } = 2000;

    public int TimeoutSeconds { get; set; } = 15;

    public AzureAiOptions Azure { get; set; } = new();

    public GroqAiOptions Groq { get; set; } = new();
}

public sealed class AzureAiOptions
{
    public string? Endpoint { get; set; }

    public string? Deployment { get; set; }

    public string? ApiKey { get; set; }

    public string ApiVersion { get; set; } = "2024-02-15-preview";
}

public sealed class GroqAiOptions
{
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";

    public string Model { get; set; } = "openai/gpt-oss-20b";

    public string? ApiKey { get; set; }

    public int MaxCompletionTokens { get; set; } = 900;
}
