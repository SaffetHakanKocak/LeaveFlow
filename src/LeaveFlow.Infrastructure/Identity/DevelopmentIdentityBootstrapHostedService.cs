using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Domain.Identity;
using LeaveFlow.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeaveFlow.Infrastructure.Identity;

internal sealed class DevelopmentIdentityBootstrapHostedService(
    IHostEnvironment environment,
    IConfiguration configuration,
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
        }
        catch (Exception)
        {
            logger.LogWarning("Development identity bootstrap was skipped because the database is not available.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task BootstrapAsync(
        DevelopmentIdentityRepository repository,
        IPasswordHashingService passwordHasher,
        CancellationToken cancellationToken)
    {
        Guid? consultantId = null;

        var consultantUserId = await TryUpsertAsync(
            repository,
            passwordHasher,
            "consultant@leaveflow.local",
            "Demo Consultant",
            RoleNames.Consultant,
            "Consultant",
            cancellationToken);

        if (consultantUserId is Guid consultantUser)
        {
            consultantId = await repository.EnsureConsultantForUserAsync(consultantUser, cancellationToken);
        }

        var managerUserId = await TryUpsertAsync(
            repository,
            passwordHasher,
            "manager@leaveflow.local",
            "Demo Manager",
            RoleNames.Manager,
            "Manager",
            cancellationToken);

        if (managerUserId is Guid managerUser)
        {
            var managerId = await repository.EnsureManagerForUserAsync(managerUser, cancellationToken);
            if (consultantId is Guid assignedConsultantId)
            {
                await repository.EnsureManagerConsultantAssignmentAsync(managerId, assignedConsultantId, cancellationToken);
            }
        }

        _ = await TryUpsertAsync(
            repository,
            passwordHasher,
            "administrator@leaveflow.local",
            "Demo Administrator",
            RoleNames.Administrator,
            "Administrator",
            cancellationToken);
    }

    private async Task<Guid?> TryUpsertAsync(
        DevelopmentIdentityRepository repository,
        IPasswordHashingService passwordHasher,
        string email,
        string displayName,
        string roleName,
        string passwordKey,
        CancellationToken cancellationToken)
    {
        var password = configuration[$"LeaveFlow:Development:Passwords:{passwordKey}"];
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
