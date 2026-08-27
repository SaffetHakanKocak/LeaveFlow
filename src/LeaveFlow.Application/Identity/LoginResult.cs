namespace LeaveFlow.Application.Identity;

public sealed class LoginResult
{
    public bool Succeeded { get; private init; }

    public AuthenticatedUser? User { get; private init; }

    public static LoginResult Success(AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new LoginResult { Succeeded = true, User = user };
    }

    public static LoginResult Failed() => new() { Succeeded = false };
}
