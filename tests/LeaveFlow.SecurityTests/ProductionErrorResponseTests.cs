extern alias ApiHost;

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ApiProgram = ApiHost::Program;

namespace LeaveFlow.SecurityTests;

public sealed class ProductionErrorResponseTests
{
    [Fact]
    public async Task UnhandledExceptionResponse_Should_NotLeak_InternalDetails()
    {
        await using var factory = new WebApplicationFactory<ApiProgram>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/_test/throw");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("Sensitive diagnostic exception marker", body);
        Assert.DoesNotContain("InvalidOperationException", body);
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
    }
}
