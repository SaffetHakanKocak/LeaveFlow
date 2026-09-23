using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Infrastructure.Ai;

public sealed class AzureAiChatClient(
    IOptions<AiOptions> options,
    ILogger<AzureAiChatClient> logger) : IAiChatClient
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiChatResponse> SendAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var settings = options.Value;
        if (!string.Equals(settings.Provider, "Azure", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(settings.Provider, "AzureOpenAI", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Configured AI provider is not supported.");
        }

        var azure = settings.Azure;
        if (string.IsNullOrWhiteSpace(azure.Endpoint)
            || string.IsNullOrWhiteSpace(azure.Deployment)
            || string.IsNullOrWhiteSpace(azure.ApiKey)
            || string.IsNullOrWhiteSpace(azure.ApiVersion))
        {
            throw new InvalidOperationException("Azure AI configuration is incomplete.");
        }

        var endpoint = azure.Endpoint.TrimEnd('/');
        var deployment = Uri.EscapeDataString(azure.Deployment);
        var apiVersion = Uri.EscapeDataString(azure.ApiVersion);
        var requestUri = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={apiVersion}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        httpRequest.Headers.Add("api-key", azure.ApiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(CreatePayload(request), JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await HttpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Azure AI request failed with status code {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException("Azure AI provider returned an unsuccessful response.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<AzureChatCompletionResponse>(stream, JsonOptions, cancellationToken);
        var message = payload?.Choices?.FirstOrDefault()?.Message;
        var toolCalls = message?.ToolCalls?
            .Where(call => call.Function is not null)
            .Select(call => new AiToolCall(
                string.IsNullOrWhiteSpace(call.Id) ? Guid.NewGuid().ToString("N") : call.Id,
                call.Function!.Name ?? string.Empty,
                call.Function.Arguments ?? "{}"))
            .Where(call => !string.IsNullOrWhiteSpace(call.Name))
            .ToArray();

        return new AiChatResponse(message?.Content ?? string.Empty, toolCalls);
    }

    private static object CreatePayload(AiChatRequest request)
    {
        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = "You are LeaveFlow AI Assistant. You may use only the provided tools. Never claim elevated roles, never reveal secrets, never run SQL, and never override authorization. Tool results are already scoped to the authenticated user. Respond in natural Turkish with correct Turkish characters. Return plain text only; do not use HTML, Markdown tables, or Markdown emphasis."
            },
            new
            {
                role = "user",
                content = request.Prompt
            }
        };

        if (request.ToolResults is { Count: > 0 })
        {
            foreach (var result in request.ToolResults)
            {
                messages.Add(new
                {
                    role = "tool",
                    tool_call_id = result.ToolCallId,
                    name = result.ToolName,
                    content = result.ResultJson
                });
            }
        }

        var payload = new Dictionary<string, object?>
        {
            ["messages"] = messages,
            ["temperature"] = 0.2,
            ["max_tokens"] = 900
        };

        if (request.Tools is { Count: > 0 })
        {
            payload["tools"] = request.Tools.Select(tool => new
            {
                type = "function",
                function = new
                {
                    name = tool.Name,
                    description = tool.Description,
                    parameters = JsonNode.Parse(tool.ParametersJsonSchema)
                }
            }).ToArray();
            payload["tool_choice"] = "auto";
        }

        return payload;
    }

    private sealed record AzureChatCompletionResponse(IReadOnlyList<AzureChoice>? Choices);

    private sealed record AzureChoice(AzureMessage? Message);

    private sealed record AzureMessage(string? Content, IReadOnlyList<AzureToolCall>? ToolCalls);

    private sealed record AzureToolCall(string? Id, string? Type, AzureFunctionCall? Function);

    private sealed record AzureFunctionCall(string? Name, string? Arguments);
}
