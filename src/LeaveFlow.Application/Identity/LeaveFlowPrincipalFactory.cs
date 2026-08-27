using System.Security.Claims;

namespace LeaveFlow.Application.Identity;

public static class LeaveFlowPrincipalFactory
{
    public static ClaimsPrincipal Create(AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName)
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (user.ConsultantId is Guid consultantId)
        {
            claims.Add(new Claim(LeaveFlowClaimTypes.ConsultantId, consultantId.ToString()));
        }

        if (user.ManagerId is Guid managerId)
        {
            claims.Add(new Claim(LeaveFlowClaimTypes.ManagerId, managerId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, LeaveFlowAuthenticationSchemes.WebCookie);
        return new ClaimsPrincipal(identity);
    }
}
