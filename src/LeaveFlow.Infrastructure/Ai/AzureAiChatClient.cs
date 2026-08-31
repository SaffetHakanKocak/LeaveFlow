using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
        var message = payload?.Choices?.FirstOrDefault()?.Message?.Content;
        return new AiChatResponse(message ?? string.Empty);
    }

    private static object CreatePayload(AiChatRequest request)
    {
        return new
        {
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You are LeaveFlow AI Assistant. Answer general leave-management questions clearly. Do not claim access to LeaveFlow data, do not ask for secrets, and do not run tools or SQL."
                },
                new
                {
                    role = "user",
                    content = request.Prompt
                }
            },
            temperature = 0.2,
            max_tokens = 700
        };
    }

    private sealed record AzureChatCompletionResponse(IReadOnlyList<AzureChoice>? Choices);

    private sealed record AzureChoice(AzureMessage? Message);

    private sealed record AzureMessage(string? Content);
}
