using LeaveFlow.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveFlow.Application.Authorization;

public static class AuthorizationPolicies
{
    public const string RequireConsultant = "RequireConsultant";
    public const string RequireManager = "RequireManager";
    public const string RequireAdministrator = "RequireAdministrator";
}

public static class LeaveFlowAuthorizationExtensions
{
    public static IServiceCollection AddLeaveFlowAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.RequireConsultant,
                policy => policy.RequireAuthenticatedUser().RequireRole(RoleNames.Consultant));

            options.AddPolicy(
                AuthorizationPolicies.RequireManager,
                policy => policy.RequireAuthenticatedUser().RequireRole(RoleNames.Manager));

            options.AddPolicy(
                AuthorizationPolicies.RequireAdministrator,
                policy => policy.RequireAuthenticatedUser().RequireRole(RoleNames.Administrator));
        });

        return services;
    }
}
