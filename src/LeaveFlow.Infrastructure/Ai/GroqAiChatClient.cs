using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Infrastructure.Ai;

public sealed class GroqAiChatClient(
    IOptions<AiOptions> options,
    ILogger<GroqAiChatClient> logger) : IAiChatClient
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiChatResponse> SendAsync(AiChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var settings = options.Value;
        if (!string.Equals(settings.Provider, "Groq", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Configured AI provider is not supported.");
        }

        var groq = settings.Groq;
        if (string.IsNullOrWhiteSpace(groq.BaseUrl)
            || string.IsNullOrWhiteSpace(groq.Model)
            || string.IsNullOrWhiteSpace(groq.ApiKey))
        {
            throw new InvalidOperationException("Groq AI configuration is incomplete.");
        }

        var baseUrl = groq.BaseUrl.TrimEnd('/');
        var requestUri = $"{baseUrl}/chat/completions";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", groq.ApiKey);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(CreatePayload(request, groq), JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await HttpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Groq AI request failed with status code {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException("Groq AI provider returned an unsuccessful response.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<GroqChatCompletionResponse>(stream, JsonOptions, cancellationToken);
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

    private static object CreatePayload(AiChatRequest request, GroqAiOptions options)
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
            ["model"] = options.Model,
            ["messages"] = messages,
            ["temperature"] = 0.2,
            ["max_completion_tokens"] = Math.Clamp(options.MaxCompletionTokens, 1, 4096)
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

    private sealed record GroqChatCompletionResponse(IReadOnlyList<GroqChoice>? Choices);

    private sealed record GroqChoice(GroqMessage? Message);

    private sealed record GroqMessage(string? Content, IReadOnlyList<GroqToolCall>? ToolCalls);

    private sealed record GroqToolCall(string? Id, string? Type, GroqFunctionCall? Function);

    private sealed record GroqFunctionCall(string? Name, string? Arguments);
}
