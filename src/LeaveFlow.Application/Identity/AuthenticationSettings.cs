namespace LeaveFlow.Application.Identity;

public sealed class AuthenticationSettings
{
    public const string SectionName = "LeaveFlow:Authentication";

    public int MaxFailedAccessAttempts { get; set; } = 5;

    public int LockoutDurationMinutes { get; set; } = 15;

    public int MaxPasswordLength { get; set; } = 256;

    public CookieAuthenticationSettings Cookie { get; set; } = new();
}

public sealed class CookieAuthenticationSettings
{
    public string Name { get; set; } = ".LeaveFlow.Auth";

    public int ExpireTimeSpanMinutes { get; set; } = 480;

    public bool SlidingExpiration { get; set; } = true;
}
