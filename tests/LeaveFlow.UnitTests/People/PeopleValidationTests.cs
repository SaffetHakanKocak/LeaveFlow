using LeaveFlow.Application.People;

namespace LeaveFlow.UnitTests.People;

public sealed class PeopleValidationTests
{
    [Fact]
    public void ConsultantValidation_Should_RequireNamesAndValidEmail()
    {
        var result = PeopleValidationAccessor.ValidateConsultant(new ConsultantInput(
            string.Empty,
            string.Empty,
            "not-an-email",
            null,
            null,
            null,
            true));

        Assert.False(result.IsValid);
        Assert.Contains("FirstName", result.Errors.Keys);
        Assert.Contains("LastName", result.Errors.Keys);
        Assert.Contains("Email", result.Errors.Keys);
    }

    [Fact]
    public void ManagerValidation_Should_AcceptValidInput()
    {
        var result = PeopleValidationAccessor.ValidateManager(new ManagerInput(
            "Ada",
            "Lovelace",
            "ada.lovelace@leaveflow.test",
            "Engineering",
            true));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

file static class PeopleValidationAccessor
{
    internal static ValidationResult ValidateConsultant(ConsultantInput input)
    {
        var service = new ConsultantManagementService(
            new StubConsultantRepository(),
            new StubConsultantAuthorizationService());

        return service.Validate(input);
    }

    internal static ValidationResult ValidateManager(ManagerInput input)
    {
        var service = new ManagerManagementService(new StubManagerRepository());
        return service.Validate(input);
    }
}
