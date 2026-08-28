using LeaveFlow.Application.Abstractions.Authorization;
using LeaveFlow.Application.Abstractions.Calendar;
using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.Calendar;
using LeaveFlow.Application.Holidays;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Application.People;
using LeaveFlow.Application.Timeline;
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
        services.AddScoped<IOrganizationHolidayService, OrganizationHolidayService>();
        services.AddScoped<IOfficialHolidayService, OfficialHolidayService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<ILeaveReviewService, LeaveReviewService>();
        services.AddScoped<IWorkforceTimelineService, WorkforceTimelineService>();
        services.AddScoped<IOrganizationCalendarService, OrganizationCalendarService>();

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
