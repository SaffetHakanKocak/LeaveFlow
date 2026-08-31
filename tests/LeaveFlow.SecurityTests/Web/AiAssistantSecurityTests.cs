using System.Net;
using System.Text.RegularExpressions;
using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Ai;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeaveFlow.SecurityTests.Web;

public sealed class AiAssistantSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public AiAssistantSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnonymousUser_Should_BeRedirected_FromAiAssistant()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/AiAssistant");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AuthenticatedUser_Should_SeeDisabledAiAssistant_WhenFeatureFlagIsOff()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/AiAssistant");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("AI Asistan", html);
        Assert.Contains("u anda kapal", html);
    }

    [Fact]
    public async Task AiAssistantPost_WithoutAntiforgeryToken_Should_BeRejected()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.PostAsync("/AiAssistant", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Prompt"] = "Merhaba"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AiAssistantPost_Should_RunToolThroughAuthenticatedHttpFlow_WhenEnabled()
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AI:Enabled"] = "true",
                    ["AI:MaxPromptLength"] = "2000",
                    ["AI:TimeoutSeconds"] = "5"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAiChatClient>();
                services.AddSingleton<IAiChatClient>(new ToolCallingAiChatClient());
            });
        });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);
        using var page = await client.GetAsync("/AiAssistant");
        var token = ExtractAntiforgeryToken(await page.Content.ReadAsStringAsync());

        using var response = await client.PostAsync("/AiAssistant", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Prompt"] = "Izin ozetimi getir",
            ["__RequestVerificationToken"] = token
        }));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Guvenli ozet hazir", html);
        Assert.Contains("Kullanilan guvenli arac sayisi: 1", html);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
    {
        using var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);

        return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        }));
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"|value=\"([^\"]+)\"[^>]*name=\"__RequestVerificationToken\"",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success);
        return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
    }

    private sealed class ToolCallingAiChatClient : IAiChatClient
    {
        private int _callCount;

        public Task<AiChatResponse> SendAsync(AiChatRequest request, CancellationToken cancellationToken = default)
        {
            _callCount++;
            return Task.FromResult(_callCount == 1
                ? new AiChatResponse("", [new AiToolCall("call-1", "GetMyLeaveSummary", "{}")])
                : new AiChatResponse("Guvenli ozet hazir."));
        }
    }
}
