using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Holidays;

namespace LeaveFlow.UnitTests.Holidays;

public sealed class HolidayDateRangeTests
{
    [Fact]
    public void GenerateInclusive_Should_ReturnSingleDay()
    {
        var days = HolidayDateRange.GenerateInclusive(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 1));

        Assert.Equal([new DateOnly(2026, 5, 1)], days);
    }

    [Fact]
    public void GenerateInclusive_Should_CrossMonthBoundary()
    {
        var days = HolidayDateRange.GenerateInclusive(new DateOnly(2026, 1, 30), new DateOnly(2026, 2, 2));

        Assert.Equal(
            [new DateOnly(2026, 1, 30), new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 2)],
            days);
    }

    [Fact]
    public void GenerateInclusive_Should_CrossYearBoundary()
    {
        var days = HolidayDateRange.GenerateInclusive(new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 1));

        Assert.Equal([new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 1)], days);
    }

    [Fact]
    public void GenerateInclusive_Should_IncludeLeapDay()
    {
        var days = HolidayDateRange.GenerateInclusive(new DateOnly(2028, 2, 28), new DateOnly(2028, 3, 1));

        Assert.Contains(new DateOnly(2028, 2, 29), days);
        Assert.Equal(3, days.Count);
    }

    [Fact]
    public void GenerateInclusive_Should_RejectInvalidRange()
    {
        Assert.Throws<ArgumentException>(() => HolidayDateRange.GenerateInclusive(new DateOnly(2026, 5, 2), new DateOnly(2026, 5, 1)));
    }

    [Fact]
    public void OrganizationValidation_Should_RequireNameAndValidRange()
    {
        var service = new OrganizationHolidayService(new StubOrganizationHolidayRepository());
        var result = service.Validate(new OrganizationHolidayInput(string.Empty, new DateOnly(2026, 5, 2), new DateOnly(2026, 5, 1), true));

        Assert.False(result.IsValid);
        Assert.Contains("Name", result.Errors.Keys);
        Assert.Contains("EndDate", result.Errors.Keys);
    }

    [Fact]
    public void OfficialValidation_Should_AcceptValidRange()
    {
        var service = new OfficialHolidayService(new StubOfficialHolidayRepository());
        var result = service.Validate(new OfficialHolidayInput("Founders Day", new DateOnly(2026, 4, 10), new DateOnly(2026, 4, 12)));

        Assert.True(result.IsValid);
    }
}
