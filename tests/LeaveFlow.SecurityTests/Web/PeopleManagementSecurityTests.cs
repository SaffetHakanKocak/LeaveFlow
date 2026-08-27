using System.Net;
using System.Text.RegularExpressions;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.People;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class PeopleManagementSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public PeopleManagementSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consultant_Should_NotAccess_OtherConsultantDetails()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var denied = await client.GetAsync($"/Consultants/Details/{_factory.ConsultantTwoId}");
        using var allowed = await client.GetAsync($"/Consultants/Details/{_factory.ConsultantOneId}");

        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("/Account/AccessDenied", denied.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    [Fact]
    public async Task Manager_Should_NotAccess_UnassignedConsultantDetails()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var denied = await client.GetAsync($"/Consultants/Details/{_factory.ConsultantTwoId}");
        using var allowed = await client.GetAsync($"/Consultants/Details/{_factory.ConsultantOneId}");

        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("/Account/AccessDenied", denied.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    [Fact]
    public async Task ConsultantManagement_Should_RequireAdministrator()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/Consultants");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Administrator_Should_AccessManagementScreens()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var consultants = await client.GetAsync("/Consultants");
        using var managers = await client.GetAsync("/Managers");

        Assert.Equal(HttpStatusCode.OK, consultants.StatusCode);
        Assert.Equal(HttpStatusCode.OK, managers.StatusCode);
    }

    [Fact]
    public async Task ConsultantCreatePost_WithoutAntiforgeryToken_Should_BeRejected()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var response = await client.PostAsync("/Consultants/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FirstName"] = "Grace",
            ["LastName"] = "Hopper",
            ["Email"] = "grace.hopper@leaveflow.test",
            ["IsActive"] = "true"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Assignment_Should_RejectInactiveConsultant()
    {
        var consultantRepository = (IConsultantManagementRepository)_factory.Store;
        var inactiveConsultantId = await consultantRepository.CreateAsync(new ConsultantInput(
            "Inactive",
            "Consultant",
            $"inactive.assignment.{Guid.NewGuid():N}@leaveflow.test",
            null,
            null,
            null,
            false));

        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);
        using var response = await PostAssignmentAsync(client, _factory.ManagerOneId, inactiveConsultantId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Assignment_Should_PreventDuplicateAssignment()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);
        using var response = await PostAssignmentAsync(client, _factory.ManagerOneId, _factory.ConsultantOneId);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains($"/Managers/Assignments/{_factory.ManagerOneId}", response.Headers.Location?.ToString());
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

    private static async Task<HttpResponseMessage> PostAssignmentAsync(HttpClient client, Guid managerId, Guid consultantId)
    {
        using var assignmentPage = await client.GetAsync($"/Managers/Assignments/{managerId}");
        var html = await assignmentPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);

        return await client.PostAsync("/Managers/Assign", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["managerId"] = managerId.ToString(),
            ["consultantId"] = consultantId.ToString(),
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
