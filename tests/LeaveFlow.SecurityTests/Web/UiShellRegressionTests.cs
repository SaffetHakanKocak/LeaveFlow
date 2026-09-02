using System.Net;
using System.Text.RegularExpressions;
using LeaveFlow.Web;
using LeaveFlow.Web.Branding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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
    public async Task LoginPage_Should_RenderDefaultLogoBranding()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("LeaveFlow'a giriş yap", html);
        Assert.Contains("lf-login-logo", html);
        Assert.Contains("/images/leaveflow.png", html);
        Assert.Contains("data-lf-theme-toggle", html);
    }

    [Fact]
    public async Task CustomBranding_Should_RenderConfiguredOrganizationAndProductName()
    {
        using var factory = CreateFactoryWithBranding(new Dictionary<string, string?>
        {
            ["LeaveFlow:Branding:OrganizationName"] = "Contoso People",
            ["LeaveFlow:Branding:ProductName"] = "PeopleOps",
            ["LeaveFlow:Branding:ShortName"] = "PO",
            ["LeaveFlow:Branding:PrimaryBrandColor"] = "#0f766e",
            ["LeaveFlow:Branding:FooterText"] = "Contoso support",
            ["LeaveFlow:Branding:SupportEmail"] = "support@example.test"
        });
        using var client = CreateClient(factory);
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Dashboard");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("PeopleOps", html);
        Assert.Contains("Contoso People kapsamı", html);
        Assert.Contains("--lf-primary: #0f766e", html);
        Assert.Contains("Contoso support", html);
        Assert.Contains("mailto:support@example.test", html);
    }

    [Fact]
    public async Task MissingLogo_Should_FallBackToConfiguredShortNameText()
    {
        using var factory = CreateFactoryWithBranding(new Dictionary<string, string?>
        {
            ["LeaveFlow:Branding:ProductName"] = "Workforce Hub",
            ["LeaveFlow:Branding:ShortName"] = "WH",
            ["LeaveFlow:Branding:LogoUrl"] = ""
        });
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("WH</span>", html);
        Assert.DoesNotContain("lf-login-logo", html);
    }

    [Fact]
    public void InvalidColor_Should_NotBecomeCustomCss()
    {
        var branding = BrandingViewModel.From(new BrandingOptions
        {
            PrimaryBrandColor = "red; background: url(javascript:alert(1))"
        });

        Assert.False(branding.HasCustomPrimaryBrandColor);
        Assert.Null(branding.PrimaryBrandColor);
    }

    [Fact]
    public async Task Layout_Should_KeepDarkModeBootstrapThemeScript()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("prefers-color-scheme: dark", html);
        Assert.Contains("data-bs-theme", html);
        Assert.Contains("leaveflow-theme", html);
    }

    [Fact]
    public async Task UserSuppliedBranding_Should_NotRenderHtmlCssOrJavascriptInjection()
    {
        using var factory = CreateFactoryWithBranding(new Dictionary<string, string?>
        {
            ["LeaveFlow:Branding:OrganizationName"] = "<script>alert(1)</script>",
            ["LeaveFlow:Branding:ProductName"] = "</title><script>alert(2)</script>",
            ["LeaveFlow:Branding:ShortName"] = "<img",
            ["LeaveFlow:Branding:LogoUrl"] = "javascript:alert(3)",
            ["LeaveFlow:Branding:PrimaryBrandColor"] = "#fff; background:url(javascript:alert(4))",
            ["LeaveFlow:Branding:SupportEmail"] = "javascript:alert(5)",
            ["LeaveFlow:Branding:FooterText"] = "<img src=x onerror=alert(6)>"
        });
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("</title><script>alert(2)</script>", html);
        Assert.DoesNotContain("<img src=x onerror=alert(6)>", html);
        Assert.DoesNotContain("javascript:alert", html);
        Assert.DoesNotContain("background:url", html);
        Assert.DoesNotContain("#fff; background", html);
        Assert.DoesNotContain("lf-login-logo", html);
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

    private static HttpClient CreateClient(WebApplicationFactory<WebEntryPoint> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private WebApplicationFactory<WebEntryPoint> CreateFactoryWithBranding(IReadOnlyDictionary<string, string?> values)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(values);
            });
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
