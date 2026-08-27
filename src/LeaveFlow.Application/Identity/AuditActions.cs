namespace LeaveFlow.Application.Identity;

public static class AuditActions
{
    public const string LoginSuccess = "Authentication.LoginSuccess";
    public const string LoginFailure = "Authentication.LoginFailure";
    public const string Lockout = "Authentication.Lockout";
    public const string Logout = "Authentication.Logout";
}

public static class AuditOutcomes
{
    public const string Success = "Success";
    public const string Failure = "Failure";
    public const string Denied = "Denied";
}
