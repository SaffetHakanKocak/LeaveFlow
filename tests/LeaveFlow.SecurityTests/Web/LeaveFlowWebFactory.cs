using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Identity;
using LeaveFlow.Domain.Identity;
using LeaveFlow.Infrastructure.Identity;
using LeaveFlow.SecurityTests.Fakes;
using LeaveFlow.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveFlow.SecurityTests.Web;

public sealed class LeaveFlowWebFactory : WebApplicationFactory<WebEntryPoint>
{
    public InMemoryIdentityStore Store { get; }
    public InMemoryHolidayStore HolidayStore { get; } = new();
    public InMemoryLeaveRequestStore LeaveRequestStore { get; } = new();
    public InMemoryWorkforceTimelineStore TimelineStore { get; } = new();
    public AspNetPasswordHashingService Hasher { get; } = new();

    public const string Password = "Test.Passw0rd!";

    public Guid ConsultantOneId { get; } = Guid.NewGuid();
    public Guid ConsultantTwoId { get; } = Guid.NewGuid();
    public Guid InactiveProfileConsultantId { get; } = Guid.NewGuid();
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
    public string InactiveProfileEmail { get; } = "inactive.profile@leaveflow.test";
    public string LockedEmail { get; } = "locked@leaveflow.test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var keyDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LeaveFlowSecurityTestsKeys"));
            services.AddDataProtection()
                .PersistKeysToFileSystem(keyDirectory)
                .SetApplicationName("LeaveFlow.SecurityTests");

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
            services.AddSingleton<IOrganizationHolidayRepository>(HolidayStore);
            services.AddSingleton<IOfficialHolidayRepository>(HolidayStore);
            services.AddSingleton<ILeaveRequestRepository>(LeaveRequestStore);
            services.AddSingleton<IWorkforceTimelineRepository>(TimelineStore);
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
        var inactiveProfile = AddUser(InactiveProfileEmail, "Inactive Profile", true, [RoleNames.Consultant]);
        var locked = AddUser(LockedEmail, "Locked User", true, [RoleNames.Consultant]);

        Store.SetConsultant(consultantOne.Id, ConsultantOneId);
        Store.SetConsultant(consultantTwo.Id, ConsultantTwoId);
        Store.SetConsultant(inactiveProfile.Id, InactiveProfileConsultantId);
        TimelineStore.AddConsultant(ConsultantOneId, "Consultant One", ConsultantOneEmail);
        TimelineStore.AddConsultant(ConsultantTwoId, "Consultant Two", ConsultantTwoEmail);
        TimelineStore.AddConsultant(InactiveProfileConsultantId, "Inactive Profile", InactiveProfileEmail, isActive: false);
        _ = ((IConsultantManagementRepository)Store).SetActiveAsync(InactiveProfileConsultantId, false).GetAwaiter().GetResult();
        Store.SetConsultant(locked.Id, Guid.NewGuid());

        Store.SetManager(managerOne.Id, ManagerOneId);
        Store.SetManager(managerTwo.Id, ManagerTwoId);
        Store.Assign(ManagerOneId, ConsultantOneId);
        Store.Assign(ManagerTwoId, ConsultantTwoId);
        LeaveRequestStore.AssignReviewer(ManagerOneId, ConsultantOneId);
        LeaveRequestStore.AssignReviewer(ManagerTwoId, ConsultantTwoId);
        TimelineStore.AssignManager(ManagerOneId, ConsultantOneId);
        TimelineStore.AssignManager(ManagerTwoId, ConsultantTwoId);
        TimelineStore.AddApprovedLeave(ConsultantOneId, "Manager one visible leave", new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 11));
        TimelineStore.AddApprovedLeave(ConsultantTwoId, "Manager two hidden leave", new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 12));

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
