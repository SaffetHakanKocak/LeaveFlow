namespace LeaveFlow.Application.Identity;

public static class LoginFailureReasons
{
    public const string InvalidCredentials = "InvalidCredentials";
    public const string UserNotFound = "UserNotFound";
    public const string InactiveAccount = "InactiveAccount";
    public const string LockedOut = "LockedOut";
}
