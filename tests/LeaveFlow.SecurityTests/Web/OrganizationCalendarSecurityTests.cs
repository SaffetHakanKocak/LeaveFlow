using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class OrganizationCalendarSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public OrganizationCalendarSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnonymousUser_Should_BeRedirected_FromOrganizationCalendar()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/OrganizationCalendar");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Consultant_Should_SeeOwnLeaveAndHolidays_ButNoOtherConsultantData()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/OrganizationCalendar?startDate=2026-09-01&endDate=2026-09-30");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("İznim", html);
        Assert.Contains("Company Calendar Day", html);
        Assert.Contains("Public Calendar Day", html);
        Assert.DoesNotContain("Consultant Two", html);
        Assert.DoesNotContain("Calendar manager two hidden leave", html);
    }

    [Fact]
    public async Task Consultant_Should_AccessOwnLeaveDetail()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/OrganizationCalendar/Details?eventType=Leave&eventId={_factory.CalendarConsultantOneLeaveId}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("İznim", html);
        Assert.Contains("Calendar manager one visible leave", html);
        Assert.DoesNotContain("Consultant Two", html);
    }

    [Fact]
    public async Task Manager_Should_SeeAssignedLeaveAndHolidays_ButNoOtherTeamLeave()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/OrganizationCalendar?startDate=2026-09-01&endDate=2026-09-30");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Consultant One leave", html);
        Assert.Contains("Company Calendar Day", html);
        Assert.Contains("Public Calendar Day", html);
        Assert.DoesNotContain("Consultant Two leave", html);
        Assert.DoesNotContain("Calendar manager two hidden leave", html);
    }

    [Fact]
    public async Task ConsultantTamperedConsultantFilter_Should_BeIgnored()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/OrganizationCalendar?startDate=2026-09-01&endDate=2026-09-30&consultantId={_factory.ConsultantTwoId}&managerId={_factory.ManagerTwoId}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("İznim", html);
        Assert.DoesNotContain("Consultant Two", html);
        Assert.DoesNotContain("Calendar manager two hidden leave", html);
    }

    [Fact]
    public async Task ManagerTamperedManagerFilter_Should_NotExpandScope()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/OrganizationCalendar?startDate=2026-09-01&endDate=2026-09-30&managerId={_factory.ManagerTwoId}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Consultant One leave", html);
        Assert.DoesNotContain("Consultant Two leave", html);
    }

    [Fact]
    public async Task Consultant_Should_NotAccess_OtherConsultantLeaveDetail()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/OrganizationCalendar/Details?eventType=Leave&eventId={_factory.CalendarConsultantTwoLeaveId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Manager_Should_NotAccess_OtherTeamLeaveDetail()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/OrganizationCalendar/Details?eventType=Leave&eventId={_factory.CalendarConsultantTwoLeaveId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Consultant_Should_AccessHolidayDetail()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/OrganizationCalendar/Details?eventType=OrganizationHoliday&eventId={_factory.CalendarOrganizationHolidayId}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Company Calendar Day", html);
        Assert.DoesNotContain("Consultant Two", html);
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
