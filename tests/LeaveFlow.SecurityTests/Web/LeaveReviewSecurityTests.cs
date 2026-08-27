using System.Net;
using System.Text.RegularExpressions;
using LeaveFlow.Domain.LeaveRequests;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LeaveFlow.SecurityTests.Web;

public sealed class LeaveReviewSecurityTests : IClassFixture<LeaveFlowWebFactory>
{
    private readonly LeaveFlowWebFactory _factory;

    public LeaveReviewSecurityTests(LeaveFlowWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consultant_Should_BeDenied_FromApprovalQueue()
    {
        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ConsultantOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync("/LeaveReviews");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Manager_Should_NotReview_OtherTeamRequest()
    {
        var request = _factory.LeaveRequestStore.Add(
            _factory.ConsultantTwoId,
            "Other team request",
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 11));

        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/LeaveReviews/Details/{request.Id}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Administrator_Should_ReviewAnyRequest()
    {
        var request = _factory.LeaveRequestStore.Add(
            _factory.ConsultantTwoId,
            "Admin review request",
            new DateOnly(2026, 10, 12),
            new DateOnly(2026, 10, 13));

        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.AdministratorEmail, LeaveFlowWebFactory.Password);

        using var response = await client.GetAsync($"/LeaveReviews/Details/{request.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApprovePost_WithoutAntiforgeryToken_Should_BeRejected()
    {
        var request = _factory.LeaveRequestStore.Add(
            _factory.ConsultantOneId,
            "CSRF approval request",
            new DateOnly(2026, 10, 14),
            new DateOnly(2026, 10, 15));

        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);

        using var response = await client.PostAsync($"/LeaveReviews/Approve/{request.Id}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ReviewNote"] = "No token"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Approve_Should_IgnoreOverpostedStatusAndReviewerFields()
    {
        var request = _factory.LeaveRequestStore.Add(
            _factory.ConsultantOneId,
            "Overpost approval request",
            new DateOnly(2026, 10, 16),
            new DateOnly(2026, 10, 17));

        using var client = CreateClient();
        _ = await LoginAsync(client, _factory.ManagerOneEmail, LeaveFlowWebFactory.Password);
        var token = await GetAntiforgeryTokenAsync(client, $"/LeaveReviews/Details/{request.Id}");

        using var response = await client.PostAsync($"/LeaveReviews/Approve/{request.Id}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Status"] = "Rejected",
            ["ReviewedBy"] = Guid.NewGuid().ToString(),
            ["ReviewedAt"] = "2001-01-01T00:00:00Z",
            ["ReviewNote"] = "Approved through valid reviewer",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var detail = await _factory.LeaveRequestStore.GetForReviewAsync(request.Id, Guid.NewGuid(), _factory.ManagerOneId, false);
        Assert.NotNull(detail);
        Assert.Equal(LeaveRequestStatuses.Approved, detail.Status);
        Assert.Equal("Approved through valid reviewer", detail.ReviewNote);
    }

    [Fact]
    public async Task RepeatedApproval_Should_OnlySucceedOnce_AndNotDuplicateDayRows()
    {
        var request = _factory.LeaveRequestStore.Add(
            _factory.ConsultantOneId,
            "Concurrent approval request",
            new DateOnly(2026, 10, 18),
            new DateOnly(2026, 10, 20));

        var beforeRows = _factory.LeaveRequestStore.LeaveDayRowCount;
        var approvals = await Task.WhenAll(
            _factory.LeaveRequestStore.ApproveAsync(request.Id, Guid.NewGuid(), _factory.ManagerOneId, false, new("First")),
            _factory.LeaveRequestStore.ApproveAsync(request.Id, Guid.NewGuid(), _factory.ManagerOneId, false, new("Second")));

        Assert.Single(approvals, approved => approved);
        Assert.Equal(beforeRows + 3, _factory.LeaveRequestStore.LeaveDayRowCount);
    }

    [Fact]
    public async Task RejectAfterApprove_Should_FailSafely()
    {
        var request = _factory.LeaveRequestStore.Add(
            _factory.ConsultantOneId,
            "Reject after approve request",
            new DateOnly(2026, 10, 21),
            new DateOnly(2026, 10, 22));

        _ = await _factory.LeaveRequestStore.ApproveAsync(request.Id, Guid.NewGuid(), _factory.ManagerOneId, false, new("Approved"));
        var rejected = await _factory.LeaveRequestStore.RejectAsync(request.Id, Guid.NewGuid(), _factory.ManagerOneId, false, new("Too late"));

        Assert.False(rejected);
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
}
