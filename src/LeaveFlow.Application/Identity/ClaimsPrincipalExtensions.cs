using System.Security.Claims;

namespace LeaveFlow.Application.Identity;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("The authenticated user id is missing.");
        }

        return userId;
    }
}
