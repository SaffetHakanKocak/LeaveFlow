using LeaveFlow.Application.Identity;
using LeaveFlow.Web.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace LeaveFlow.SecurityTests;

public sealed class AuthenticationCookieSecurityTests
{
    [Fact]
    public void ProductionCookieOptions_Should_BeHardened()
    {
        var options = new CookieAuthenticationOptions();
        AuthenticationCookieConfiguration.Apply(options, new AuthenticationSettings(), isProduction: true);

        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.True(options.SlidingExpiration);
        Assert.Equal(".LeaveFlow.Auth", options.Cookie.Name);
        Assert.Equal(TimeSpan.FromMinutes(480), options.ExpireTimeSpan);
    }

    [Fact]
    public void NonProductionCookieOptions_Should_UseSameAsRequestSecurePolicy()
    {
        var options = new CookieAuthenticationOptions();
        AuthenticationCookieConfiguration.Apply(options, new AuthenticationSettings(), isProduction: false);

        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
        Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
    }
}
