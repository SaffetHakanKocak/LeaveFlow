using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Identity;
using LeaveFlow.Domain.Identity;
using LeaveFlow.Infrastructure.Identity;
using LeaveFlow.SecurityTests.Fakes;
using LeaveFlow.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveFlow.SecurityTests.Web;

public sealed class LeaveFlowWebFactory : WebApplicationFactory<WebEntryPoint>
{
    public InMemoryIdentityStore Store { get; }
    public AspNetPasswordHashingService Hasher { get; } = new();

    public const string Password = "Test.Passw0rd!";

    public Guid ConsultantOneId { get; } = Guid.NewGuid();
    public Guid ConsultantTwoId { get; } = Guid.NewGuid();
    public Guid ManagerOneId { get; } = Guid.NewGuid();
    public Guid ManagerTwoId { get; } = Guid.NewGuid();

    public LeaveFlowWebFactory()
    {
        Store = new InMemoryIdentityStore();
        Seed();
    }

    public string ConsultantOneEmail { get; } = "consultant.one@leaveflow.test";
    public string ConsultantTwoEmail { get; } = "consultant.two@leaveflow.test";
    public string ManagerOneEmail { get; } = "manager.one@leaveflow.test";
    public string ManagerTwoEmail { get; } = "manager.two@leaveflow.test";
    public string AdministratorEmail { get; } = "administrator@leaveflow.test";
    public string InactiveEmail { get; } = "inactive@leaveflow.test";
    public string LockedEmail { get; } = "locked@leaveflow.test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(Store);
            services.AddSingleton<IUserAuthRepository>(Store);
            services.AddSingleton<IUserRoleRepository>(Store);
            services.AddSingleton<IConsultantIdentityRepository>(Store);
            services.AddSingleton<IManagerIdentityRepository>(Store);
            services.AddSingleton<ILoginAttemptRepository>(Store);
            services.AddSingleton<IAuditLogRepository>(Store);
            services.AddSingleton<IConsultantManagementRepository>(Store);
            services.AddSingleton<IManagerManagementRepository>(Store);
            services.AddSingleton<IManagerConsultantAssignmentRepository>(Store);
        });
    }

    private void Seed()
    {
        var consultantOne = AddUser(ConsultantOneEmail, "Consultant One", true, [RoleNames.Consultant]);
        var consultantTwo = AddUser(ConsultantTwoEmail, "Consultant Two", true, [RoleNames.Consultant]);
        var managerOne = AddUser(ManagerOneEmail, "Manager One", true, [RoleNames.Manager]);
        var managerTwo = AddUser(ManagerTwoEmail, "Manager Two", true, [RoleNames.Manager]);
        _ = AddUser(AdministratorEmail, "Administrator", true, [RoleNames.Administrator]);
        _ = AddUser(InactiveEmail, "Inactive User", false, [RoleNames.Consultant]);
        var locked = AddUser(LockedEmail, "Locked User", true, [RoleNames.Consultant]);

        Store.SetConsultant(consultantOne.Id, ConsultantOneId);
        Store.SetConsultant(consultantTwo.Id, ConsultantTwoId);
        Store.SetConsultant(locked.Id, Guid.NewGuid());

        Store.SetManager(managerOne.Id, ManagerOneId);
        Store.SetManager(managerTwo.Id, ManagerTwoId);
        Store.Assign(ManagerOneId, ConsultantOneId);
        Store.Assign(ManagerTwoId, ConsultantTwoId);

        var lockedUser = Store.GetUser(LockedEmail);
        Store.AddUser(
            lockedUser with { FailedLoginCount = 5, LockoutEnd = DateTime.UtcNow.AddHours(1) },
            [RoleNames.Consultant]);
    }

    private UserAuthRecord AddUser(string email, string displayName, bool active, IReadOnlyList<string> roles)
    {
        return Store.AddUser(
            new UserAuthRecord(
                Guid.NewGuid(),
                email,
                displayName,
                Hasher.HashPassword(Password),
                active,
                0,
                null,
                null),
            roles);
    }
}
