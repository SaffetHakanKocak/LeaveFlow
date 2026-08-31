using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class SecurityHardeningTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public SecurityHardeningTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublicResponses_Should_IncludeSecurityHeaders()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertHeader(response, "X-Content-Type-Options", "nosniff");
        AssertHeader(response, "X-Frame-Options", "DENY");
        AssertHeader(response, "Referrer-Policy", "no-referrer");
        AssertHeader(response, "X-Permitted-Cross-Domain-Policies", "none");
        AssertHeader(response, "Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        Assert.Contains("default-src 'self'", HeaderValue(response, "Content-Security-Policy"));
        Assert.Contains("frame-ancestors 'none'", HeaderValue(response, "Content-Security-Policy"));
        Assert.Contains("form-action 'self'", HeaderValue(response, "Content-Security-Policy"));
    }

    [Fact]
    public async Task Login_Should_NotRedirect_ToExternalReturnUrl()
    {
        using var client = CreateClient();
        using var loginPage = await client.GetAsync("/Account/Login?returnUrl=https%3A%2F%2Fevil.example%2Fsteal");
        var token = ExtractAntiforgeryToken(await loginPage.Content.ReadAsStringAsync());

        using var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = _factory.ConsultantOneEmail,
            ["Password"] = LeaveFlowWebFactory.Password,
            ["ReturnUrl"] = "https://evil.example/steal",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.False(
            response.Headers.Location?.ToString().StartsWith("https://evil.example", StringComparison.OrdinalIgnoreCase) == true,
            "Login redirected to an external returnUrl.");
    }

    [Fact]
    public async Task LogoutPost_WithoutAntiforgeryToken_Should_BeRejected()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.PostAsync("/Account/Logout", new FormUrlEncodedContent(new Dictionary<string, string>()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConsultantRoleTamperingPost_Should_NotReachAdministratorAction()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);
        var token = await GetAntiforgeryTokenAsync(client, "/LeaveRequests/Create");

        using var response = await client.PostAsync("/Managers/Assign", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["managerId"] = _factory.ManagerOneId.ToString(),
            ["consultantId"] = _factory.ConsultantOneId.ToString(),
            ["Role"] = "Administrator",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task LeaveReason_Should_BeHtmlEncoded()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);
        var token = await GetAntiforgeryTokenAsync(client, "/LeaveRequests/Create");

        using var response = await client.PostAsync("/LeaveRequests/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Reason"] = "<script>alert('leave')</script>",
            ["StartDate"] = "2026-12-10",
            ["EndDate"] = "2026-12-11",
            ["__RequestVerificationToken"] = token
        }));

        using var detail = await client.GetAsync(response.Headers.Location);
        var html = await detail.Content.ReadAsStringAsync();

        Assert.DoesNotContain("<script>alert('leave')</script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;alert", html);
    }

    [Fact]
    public async Task ReviewNote_Should_BeHtmlEncoded()
    {
        var request = _factory.LeaveRequestStore.Add(
            _factory.ConsultantOneId,
            "Review note encoding request",
            new DateOnly(2026, 12, 12),
            new DateOnly(2026, 12, 13));

        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);
        var token = await GetAntiforgeryTokenAsync(client, $"/LeaveReviews/Details/{request.Id}");

        using var response = await client.PostAsync($"/LeaveReviews/Approve/{request.Id}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ReviewNote"] = "<img src=x onerror=alert('review')>",
            ["__RequestVerificationToken"] = token
        }));

        using var detail = await client.GetAsync(response.Headers.Location);
        var html = await detail.Content.ReadAsStringAsync();

        Assert.DoesNotContain("<img src=x onerror=alert('review')>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;img src=x", html);
    }

    [Fact]
    public async Task HolidayName_Should_BeHtmlEncoded()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);
        var token = await GetAntiforgeryTokenAsync(client, "/OrganizationHolidays/Create");

        using var response = await client.PostAsync("/OrganizationHolidays/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "<img src=x onerror=alert('holiday')>",
            ["StartDate"] = "2026-12-14",
            ["EndDate"] = "2026-12-14",
            ["IsActive"] = "true",
            ["__RequestVerificationToken"] = token
        }));

        using var detail = await client.GetAsync(response.Headers.Location);
        var html = await detail.Content.ReadAsStringAsync();

        Assert.DoesNotContain("<img src=x onerror=alert('holiday')>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;img src=x", html);
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
        var token = await GetAntiforgeryTokenAsync(client, "/Account/Login");

        return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        }));
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string url)
    {
        using var page = await client.GetAsync(url);
        var html = await page.Content.ReadAsStringAsync();
        return ExtractAntiforgeryToken(html);
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

    private static void AssertHeader(HttpResponseMessage response, string headerName, string expectedValue)
    {
        Assert.Equal(expectedValue, HeaderValue(response, headerName));
    }

    private static string HeaderValue(HttpResponseMessage response, string headerName)
    {
        Assert.True(response.Headers.TryGetValues(headerName, out var values), $"{headerName} header is missing.");
        return Assert.Single(values);
    }
}
