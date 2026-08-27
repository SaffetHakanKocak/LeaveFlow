using System.Net;
using System.Text.RegularExpressions;
using LeaveFlow.Application.Identity;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class WebAuthenticationSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public WebAuthenticationSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnonymousUser_Should_BeRedirected_FromProtectedResource()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync($"/resources/consultants/{_factory.ConsultantOneId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ValidUser_Should_Login()
    {
        using var client = CreateClient();
        using var response = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), value => value.Contains(".LeaveFlow.Auth", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InvalidPassword_Should_ReturnGenericError()
    {
        using var client = CreateClient();
        using var response = await LoginAsync(client, _factory.ConsultantOneEmail, "Wrong.Passw0rd!");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(AuthenticationMessages.InvalidCredentials, body);
        Assert.DoesNotContain("not found", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("inactive", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("locked", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InactiveUser_Should_NotLogin()
    {
        using var client = CreateClient();
        using var response = await LoginAsync(client, _factory.InactiveEmail, LeaveFlowWebFactory.Password);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(AuthenticationMessages.InvalidCredentials, body);
        Assert.DoesNotContain(response.Headers, header => header.Key == "Set-Cookie" && header.Value.Any(value => value.Contains(".LeaveFlow.Auth", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task LockedUser_Should_NotLogin()
    {
        using var client = CreateClient();
        using var response = await LoginAsync(client, _factory.LockedEmail, LeaveFlowWebFactory.Password);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(AuthenticationMessages.InvalidCredentials, body);
    }

    [Fact]
    public async Task FailedLogin_Should_IncrementFailedCount_AndSuccessfulLogin_Should_Reset()
    {
        var email = $"counter.{Guid.NewGuid():N}@leaveflow.test";
        var hasher = _factory.Hasher;
        _factory.Store.AddUser(
            new UserAuthRecord(Guid.NewGuid(), email, "Counter User", hasher.HashPassword(LeaveFlowWebFactory.Password), true, 0, null, null),
            ["Consultant"]);

        using var client = CreateClient();
        _ = await LoginAsync(client, email, "Wrong.Passw0rd!");
        Assert.Equal(1, _factory.Store.GetUser(email).FailedLoginCount);

        using var success = await LoginAsync(client, email, LeaveFlowWebFactory.Password);
        Assert.Equal(HttpStatusCode.Redirect, success.StatusCode);
        Assert.Equal(0, _factory.Store.GetUser(email).FailedLoginCount);
        Assert.Null(_factory.Store.GetUser(email).LockoutEnd);
    }

    [Fact]
    public async Task Consultant_Should_NotAccess_AnotherConsultantResource()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var denied = await client.GetAsync($"/resources/consultants/{_factory.ConsultantTwoId}");
        using var allowed = await client.GetAsync($"/resources/consultants/{_factory.ConsultantOneId}");

        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("/Account/AccessDenied", denied.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    [Fact]
    public async Task Manager_Should_NotAccess_AnotherManagersConsultant()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var denied = await client.GetAsync($"/resources/consultants/{_factory.ConsultantTwoId}");
        using var allowed = await client.GetAsync($"/resources/consultants/{_factory.ConsultantOneId}");

        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("/Account/AccessDenied", denied.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    [Fact]
    public async Task Administrator_Should_AccessConsultantResource()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/resources/consultants/{_factory.ConsultantTwoId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LoginCookie_Should_BeHttpOnly_AndSameSiteLax()
    {
        using var client = CreateClient();
        using var response = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);
        var cookie = response.Headers.GetValues("Set-Cookie")
            .First(value => value.Contains(".LeaveFlow.Auth", StringComparison.OrdinalIgnoreCase));

        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginPost_WithoutAntiforgeryToken_Should_BeRejected()
    {
        using var client = CreateClient();
        using var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = _factory.ConsultantOneEmail,
            ["Password"] = LeaveFlowWebFactory.Password
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
