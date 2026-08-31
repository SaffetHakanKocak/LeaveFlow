using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Ai;
using LeaveFlow.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LeaveFlow.UnitTests.Ai;

public sealed class AiAssistantServiceTests
{
    [Fact]
    public async Task SendAsync_Should_ReturnDisabled_WhenAiFeatureFlagIsOff()
    {
        var client = new RecordingAiChatClient(new AiChatResponse("unused"));
        var service = CreateService(client, new AiOptions { Enabled = false });

        var result = await service.SendAsync(new AiAssistantInput(Guid.NewGuid(), "Test User", "Merhaba"));

        Assert.False(result.Succeeded);
        Assert.False(result.IsEnabled);
        Assert.False(client.WasCalled);
    }

    [Fact]
    public async Task SendAsync_Should_ValidatePromptLength()
    {
        var service = CreateService(
            new RecordingAiChatClient(new AiChatResponse("unused")),
            new AiOptions { Enabled = true, MaxPromptLength = 5 });

        var result = await service.SendAsync(new AiAssistantInput(Guid.NewGuid(), "Test User", "123456"));

        Assert.False(result.Succeeded);
        Assert.True(result.IsEnabled);
        Assert.Contains("en fazla 5 karakter", result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_Should_ReturnSafeFailure_WhenProviderThrows()
    {
        var service = CreateService(
            new ThrowingAiChatClient(),
            new AiOptions { Enabled = true, MaxPromptLength = 2000, TimeoutSeconds = 5 });

        var result = await service.SendAsync(new AiAssistantInput(Guid.NewGuid(), "Test User", "Merhaba"));

        Assert.False(result.Succeeded);
        Assert.True(result.IsEnabled);
        Assert.Equal("AI Asistan şu anda yanıt veremiyor. Lütfen daha sonra tekrar deneyin.", result.ErrorMessage);
    }

    [Fact]
    public async Task AzureProvider_Should_FailSafely_WhenConfigurationIsIncomplete()
    {
        var provider = new AzureAiChatClient(
            Options.Create(new AiOptions
            {
                Enabled = true,
                Provider = "Azure",
                Azure = new AzureAiOptions()
            }),
            NullLogger<AzureAiChatClient>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.SendAsync(
            new AiChatRequest(Guid.NewGuid(), "Test User", "Merhaba")));
    }

    private static AiAssistantService CreateService(IAiChatClient client, AiOptions options)
    {
        return new AiAssistantService(
            client,
            Options.Create(options),
            NullLogger<AiAssistantService>.Instance);
    }

    private sealed class RecordingAiChatClient(AiChatResponse response) : IAiChatClient
    {
        public bool WasCalled { get; private set; }

        public Task<AiChatResponse> SendAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingAiChatClient : IAiChatClient
    {
        public Task<AiChatResponse> SendAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            throw new HttpRequestException("Provider unavailable");
        }
    }
}
