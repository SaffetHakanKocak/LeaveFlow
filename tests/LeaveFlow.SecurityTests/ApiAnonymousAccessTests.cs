extern alias ApiHost;

using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ApiProgram = ApiHost::Program;

namespace LeaveFlow.SecurityTests;

public sealed class ApiAnonymousAccessTests
{
    [Fact]
    public async Task AnonymousUser_Should_NotAccess_ProtectedApiEndpoint()
    {
        await using var factory = new WebApplicationFactory<ApiProgram>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/secure/ping");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("JWT", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DeferredApiAuthenticationHandler", body);
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", body, StringComparison.OrdinalIgnoreCase);
    }
}
