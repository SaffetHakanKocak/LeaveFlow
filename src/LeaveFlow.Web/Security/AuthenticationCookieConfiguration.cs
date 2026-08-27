using LeaveFlow.Application.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LeaveFlow.Web.Security;

public static class AuthenticationCookieConfiguration
{
    public static void Apply(
        CookieAuthenticationOptions options,
        AuthenticationSettings settings,
        bool isProduction)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(settings);

        options.Cookie.Name = string.IsNullOrWhiteSpace(settings.Cookie.Name)
            ? ".LeaveFlow.Auth"
            : settings.Cookie.Name;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = isProduction
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(settings.Cookie.ExpireTimeSpanMinutes);
        options.SlidingExpiration = settings.Cookie.SlidingExpiration;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ReturnUrlParameter = "returnUrl";
    }
}
