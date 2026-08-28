using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class ReportingSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public ReportingSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnonymousUser_Should_BeRedirected_FromDashboard()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Dashboard");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task AnonymousUser_Should_BeRedirected_FromReports()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Reports");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ConsultantDashboard_Should_ShowOwnSummaryWithoutOtherConsultantData()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Dashboard");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Paneli", html);
        Assert.Contains("znim", html);
        Assert.Contains("Bekliyor", html);
        Assert.DoesNotContain("Consultant Two", html);
    }

    [Fact]
    public async Task Consultant_Should_BeDenied_FromOrganizationReports()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Reports?startDate=2026-09-01&endDate=2026-09-30");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ManagerDashboard_Should_ShowOnlyAssignedTeamData()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Dashboard");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Paneli", html);
        Assert.Contains("Consultant One", html);
        Assert.DoesNotContain("Consultant Two", html);
    }

    [Fact]
    public async Task ManagerReports_Should_ShowOnlyAssignedTeamData()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Reports?startDate=2026-09-01&endDate=2026-09-30");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Consultant One", html);
        Assert.DoesNotContain("Consultant Two", html);
    }

    [Fact]
    public async Task ManagerTamperedReportFilters_Should_NotExpandScope()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/Reports?startDate=2026-09-01&endDate=2026-09-30&managerId={_factory.ManagerTwoId}&consultantId={_factory.ConsultantTwoId}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Consultant One", html);
        Assert.DoesNotContain("Consultant Two", html);
    }

    [Fact]
    public async Task AdministratorReports_Should_ShowOrganizationData()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Reports?startDate=2026-09-01&endDate=2026-09-30");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Consultant One", html);
        Assert.Contains("Consultant Two", html);
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
