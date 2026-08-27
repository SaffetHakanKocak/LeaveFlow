using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class HolidayManagementSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public HolidayManagementSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/OrganizationHolidays")]
    [InlineData("/OfficialHolidays")]
    public async Task AnonymousUser_Should_BeRedirected_FromHolidayManagement(string url)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("/OrganizationHolidays", "consultant.one@leaveflow.test")]
    [InlineData("/OfficialHolidays", "consultant.one@leaveflow.test")]
    [InlineData("/OrganizationHolidays", "manager.one@leaveflow.test")]
    [InlineData("/OfficialHolidays", "manager.one@leaveflow.test")]
    public async Task NonAdministrator_Should_BeDenied_FromHolidayManagement(string url, string email)
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, email, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("/OrganizationHolidays")]
    [InlineData("/OfficialHolidays")]
    public async Task Administrator_Should_AccessHolidayManagement(string url)
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/OrganizationHolidays/Create")]
    [InlineData("/OfficialHolidays/Create")]
    public async Task HolidayCreatePost_WithoutAntiforgeryToken_Should_BeRejected(string url)
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var response = await client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Test Holiday",
            ["StartDate"] = "2026-05-01",
            ["EndDate"] = "2026-05-01",
            ["IsActive"] = "true"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OrganizationHolidayCreate_Should_IgnoreOverpostedId()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        var attemptedId = Guid.NewGuid();
        using var createPage = await client.GetAsync("/OrganizationHolidays/Create");
        var token = ExtractAntiforgeryToken(await createPage.Content.ReadAsStringAsync());

        using var response = await client.PostAsync("/OrganizationHolidays/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = attemptedId.ToString(),
            ["Name"] = "Overpost Holiday",
            ["StartDate"] = "2026-05-01",
            ["EndDate"] = "2026-05-02",
            ["IsActive"] = "true",
            ["CreatedAt"] = "2001-01-01",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(attemptedId.ToString(), response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
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
