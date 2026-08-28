using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class UiShellRegressionTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public UiShellRegressionTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginPage_Should_RenderProductShell_AndAntiforgeryToken()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Devam etmek", html);
        Assert.Contains("__RequestVerificationToken", html);
    }

    [Fact]
    public async Task AuthenticatedShell_Should_RenderSidebarTopbarAndThemeToggle()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Dashboard");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("lf-sidebar", html);
        Assert.Contains("lf-topbar", html);
        Assert.Contains("data-lf-theme-toggle", html);
        Assert.Contains("netici", html);
    }

    [Fact]
    public async Task ConsultantNavigation_Should_NotExposeAdminOrManagerConvenienceLinks()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Dashboard");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("zin Taleplerim", html);
        Assert.Contains("Takvim", html);
        Assert.DoesNotContain(">Raporlar<", html);
        Assert.DoesNotContain("href=\"/Managers\"", html);
        Assert.DoesNotContain(">Kurum Tatilleri<", html);
    }

    [Fact]
    public async Task AdministratorNavigation_Should_ExposeAdministrationConvenienceLinks()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Dashboard");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("manlar", html);
        Assert.Contains("neticiler", html);
        Assert.Contains(">Kurum Tatilleri<", html);
        Assert.Contains(">Raporlar<", html);
    }

    [Fact]
    public async Task CriticalForms_Should_KeepAntiforgeryTokens()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/LeaveRequests/Create");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("__RequestVerificationToken", html);
    }

    [Fact]
    public async Task UnauthorizedUiRoute_Should_StillBeDenied()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Reports");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
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
}
