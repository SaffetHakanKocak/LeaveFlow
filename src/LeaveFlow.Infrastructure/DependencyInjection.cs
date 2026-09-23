using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Calendar;
using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Ai;
using LeaveFlow.Infrastructure.Ai;
using LeaveFlow.Infrastructure.Identity;
using LeaveFlow.Infrastructure.Persistence.Repositories;
using LeaveFlow.Infrastructure.Persistence.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var databaseOptions = new DatabaseOptions
        {
            ConnectionStringName = configuration["LeaveFlow:Database:ConnectionStringName"] ?? "DefaultConnection"
        };

        if (string.IsNullOrWhiteSpace(databaseOptions.ConnectionStringName))
        {
            throw new InvalidOperationException("The database connection string name is not configured.");
        }

        SqlServerTypeHandlers.Register();

        var connectionString = configuration.GetConnectionString(databaseOptions.ConnectionStringName) ?? string.Empty;

        services.AddSingleton(databaseOptions);
        services.AddSingleton<IDbConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
        services.AddSingleton<IDataTransactionFactory>(_ => new SqlDataTransactionFactory(connectionString));
        services.AddSingleton<IPasswordHashingService, AspNetPasswordHashingService>();
        services.AddScoped<AzureAiChatClient>();
        services.AddScoped<GroqAiChatClient>();
        services.AddScoped<IAiChatClient>(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value.Provider;
            return provider switch
            {
                var value when string.Equals(value, "Groq", StringComparison.OrdinalIgnoreCase) =>
                    serviceProvider.GetRequiredService<GroqAiChatClient>(),
                var value when string.Equals(value, "Azure", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value, "AzureOpenAI", StringComparison.OrdinalIgnoreCase) =>
                    serviceProvider.GetRequiredService<AzureAiChatClient>(),
                _ => throw new InvalidOperationException("Configured AI provider is not supported.")
            };
        });
        services.AddScoped<IRoleReadRepository, RoleReadRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IConsultantIdentityRepository, ConsultantIdentityRepository>();
        services.AddScoped<IManagerIdentityRepository, ManagerIdentityRepository>();
        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IConsultantManagementRepository, ConsultantManagementRepository>();
        services.AddScoped<IManagerManagementRepository, ManagerManagementRepository>();
        services.AddScoped<IManagerConsultantAssignmentRepository, ManagerConsultantAssignmentRepository>();
        services.AddScoped<IOrganizationHolidayRepository, OrganizationHolidayRepository>();
        services.AddScoped<IOfficialHolidayRepository, OfficialHolidayRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IWorkforceTimelineRepository, WorkforceTimelineRepository>();
        services.AddScoped<IOrganizationCalendarRepository, OrganizationCalendarRepository>();
        services.AddScoped<IReportingRepository, ReportingRepository>();
        services.AddScoped<DevelopmentIdentityRepository>();
        services.AddHostedService<DevelopmentIdentityBootstrapHostedService>();

        return services;
    }
}
