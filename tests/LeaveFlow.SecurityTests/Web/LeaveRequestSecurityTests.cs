using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class LeaveRequestSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public LeaveRequestSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnonymousUser_Should_BeRedirected_FromMyLeaveRequests()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("/LeaveRequests");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("manager.one@leaveflow.test")]
    [InlineData("administrator@leaveflow.test")]
    public async Task NonConsultant_Should_BeDenied_FromMyLeaveRequests(string email)
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, email, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/LeaveRequests");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Consultant_Should_NotAccess_AnotherConsultantsLeaveRequest()
    {
        var otherRequest = _factory.LeaveRequestStore.Add(
            _factory.ConsultantTwoId,
            "Other consultant leave",
            new DateOnly(2026, 9, 20),
            new DateOnly(2026, 9, 22));

        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/LeaveRequests/Details/{otherRequest.Id}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Create_Should_IgnoreOverpostedSecurityFields()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);
        using var createPage = await client.GetAsync("/LeaveRequests/Create");
        var token = ExtractAntiforgeryToken(await createPage.Content.ReadAsStringAsync());

        using var response = await client.PostAsync("/LeaveRequests/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ConsultantId"] = _factory.ConsultantTwoId.ToString(),
            ["Status"] = "Approved",
            ["ReviewedAt"] = "2026-09-01T00:00:00Z",
            ["ReviewedBy"] = Guid.NewGuid().ToString(),
            ["CreatedAt"] = "2001-01-01T00:00:00Z",
            ["Reason"] = "Overposted request",
            ["StartDate"] = "2026-10-01",
            ["EndDate"] = "2026-10-02",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var detailUrl = response.Headers.Location?.ToString();
        Assert.NotNull(detailUrl);

        using var detail = await client.GetAsync(detailUrl);
        var body = await detail.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Contains("Bekliyor", body);
        Assert.DoesNotContain("Onaylandı", body);
    }

    [Fact]
    public async Task CreatePost_WithoutAntiforgeryToken_Should_BeRejected()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.PostAsync("/LeaveRequests/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Reason"] = "Missing token",
            ["StartDate"] = "2026-11-01",
            ["EndDate"] = "2026-11-02"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InactiveConsultantProfile_Should_NotCreateLeaveRequest()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.InactiveProfileEmail, LeaveFlowWebFactory.Password);
        using var createPage = await client.GetAsync("/LeaveRequests/Create");
        var token = ExtractAntiforgeryToken(await createPage.Content.ReadAsStringAsync());

        using var response = await client.PostAsync("/LeaveRequests/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Reason"] = "Inactive profile request",
            ["StartDate"] = "2026-12-01",
            ["EndDate"] = "2026-12-02",
            ["__RequestVerificationToken"] = token
        }));

        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("aktif bir", body, StringComparison.OrdinalIgnoreCase);
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
