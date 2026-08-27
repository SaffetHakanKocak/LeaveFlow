using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Infrastructure.Identity;
using LeaveFlow.Infrastructure.Persistence.Repositories;
using LeaveFlow.Infrastructure.Persistence.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        var connectionString = configuration.GetConnectionString(databaseOptions.ConnectionStringName) ?? string.Empty;

        services.AddSingleton(databaseOptions);
        services.AddSingleton<IDbConnectionFactory>(_ => new SqlServerConnectionFactory(connectionString));
        services.AddSingleton<IDataTransactionFactory>(_ => new SqlDataTransactionFactory(connectionString));
        services.AddSingleton<IPasswordHashingService, AspNetPasswordHashingService>();
        services.AddScoped<IRoleReadRepository, RoleReadRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IConsultantIdentityRepository, ConsultantIdentityRepository>();
        services.AddScoped<IManagerIdentityRepository, ManagerIdentityRepository>();
        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<DevelopmentIdentityRepository>();
        services.AddHostedService<DevelopmentIdentityBootstrapHostedService>();

        return services;
    }
}
