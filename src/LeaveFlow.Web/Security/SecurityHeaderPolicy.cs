namespace LeaveFlow.Web.Security;

public static class SecurityHeaderPolicy
{
    public static void Apply(HttpContext context, bool isProduction)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
        context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
        context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
        context.Response.Headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        context.Response.Headers.TryAdd("Content-Security-Policy", BuildContentSecurityPolicy(isProduction));
    }

    private static string BuildContentSecurityPolicy(bool isProduction)
    {
        var directives = new List<string>
        {
            "default-src 'self'",
            "base-uri 'self'",
            "object-src 'none'",
            "frame-ancestors 'none'",
            "img-src 'self' data:",
            "font-src 'self'",
            "style-src 'self' 'unsafe-inline'",
            "script-src 'self' 'unsafe-inline'",
            "form-action 'self'"
        };

        if (isProduction)
        {
            directives.Add("upgrade-insecure-requests");
        }

        return string.Join("; ", directives);
    }
}
