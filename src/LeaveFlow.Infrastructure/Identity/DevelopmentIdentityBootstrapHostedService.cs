using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Domain.Identity;
using LeaveFlow.Infrastructure.Persistence.Repositories;
using LeaveFlow.Infrastructure.Persistence.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeaveFlow.Infrastructure.Identity;

internal sealed class DevelopmentIdentityBootstrapHostedService(
    IHostEnvironment environment,
    IConfiguration configuration,
    DatabaseOptions databaseOptions,
    IServiceScopeFactory scopeFactory,
    ILogger<DevelopmentIdentityBootstrapHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        if (!configuration.GetValue("LeaveFlow:Development:BootstrapIdentity", false))
        {
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<DevelopmentIdentityRepository>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();
            await BootstrapAsync(repository, passwordHasher, cancellationToken);
            logger.LogInformation(
                "Development identity bootstrap completed using connection string name {ConnectionStringName}.",
                databaseOptions.ConnectionStringName);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Development identity bootstrap failed using connection string name {ConnectionStringName}. Run local database setup and confirm development stored procedures are applied.",
                databaseOptions.ConnectionStringName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task BootstrapAsync(
        DevelopmentIdentityRepository repository,
        IPasswordHashingService passwordHasher,
        CancellationToken cancellationToken)
    {
        var managerUserId = await TryUpsertAsync(
            repository,
            passwordHasher,
            "manager@leaveflow.local",
            "Demo Manager",
            RoleNames.Manager,
            "Manager",
            cancellationToken);

        Guid? managerId = null;
        if (managerUserId is Guid managerUser)
        {
            managerId = await repository.EnsureManagerForUserAsync(managerUser, cancellationToken);
        }

        foreach (var consultant in GetDemoConsultants())
        {
            var consultantUserId = await TryUpsertAsync(
                repository,
                passwordHasher,
                consultant.Email,
                consultant.DisplayName,
                RoleNames.Consultant,
                "Consultant",
                cancellationToken);

            if (consultantUserId is Guid consultantUser && managerId is Guid assignedManagerId)
            {
                var consultantId = await repository.EnsureConsultantForUserAsync(consultantUser, cancellationToken);
                await repository.EnsureManagerConsultantAssignmentAsync(assignedManagerId, consultantId, cancellationToken);
            }
        }

        _ = await TryUpsertAsync(
            repository,
            passwordHasher,
            "admin@leaveflow.local",
            "Demo Admin",
            RoleNames.Administrator,
            "Administrator",
            cancellationToken);
    }

    private static IReadOnlyList<(string Email, string DisplayName)> GetDemoConsultants() =>
    [
        ("consultant1@leaveflow.local", "Demo Consultant 1"),
        ("consultant2@leaveflow.local", "Demo Consultant 2")
    ];

    private async Task<Guid?> TryUpsertAsync(
        DevelopmentIdentityRepository repository,
        IPasswordHashingService passwordHasher,
        string email,
        string displayName,
        string roleName,
        string passwordKey,
        CancellationToken cancellationToken)
    {
        var password = configuration["LeaveFlow:Development:DemoPassword"]
            ?? configuration[$"LeaveFlow:Development:Passwords:{passwordKey}"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Development identity bootstrap skipped a user because a development credential is not configured for role {Role}.",
                roleName);
            return null;
        }

        var userId = await repository.UpsertUserAsync(
            email,
            EmailNormalizer.Normalize(email),
            displayName,
            passwordHasher.HashPassword(password),
            isActive: true,
            cancellationToken);

        await repository.EnsureRoleAsync(userId, roleName, cancellationToken);
        return userId;
    }
}
