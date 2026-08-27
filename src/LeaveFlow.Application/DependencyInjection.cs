using LeaveFlow.Application.Abstractions.Authorization;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.People;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeaveFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IConsultantResourceAuthorizationService, ConsultantResourceAuthorizationService>();
        services.AddScoped<IConsultantManagementService, ConsultantManagementService>();
        services.AddScoped<IManagerManagementService, ManagerManagementService>();

        var options = services.AddOptions<AuthenticationSettings>();
        if (configuration is not null)
        {
            options.Bind(configuration.GetSection(AuthenticationSettings.SectionName));
        }

        options
            .Validate(settings => settings.MaxFailedAccessAttempts > 0, "LeaveFlow:Authentication:MaxFailedAccessAttempts must be greater than zero.")
            .Validate(settings => settings.LockoutDurationMinutes > 0, "LeaveFlow:Authentication:LockoutDurationMinutes must be greater than zero.")
            .Validate(settings => settings.MaxPasswordLength > 0, "LeaveFlow:Authentication:MaxPasswordLength must be greater than zero.")
            .Validate(settings => settings.Cookie.ExpireTimeSpanMinutes > 0, "LeaveFlow:Authentication:Cookie:ExpireTimeSpanMinutes must be greater than zero.");

        return services;
    }
}
