namespace LeaveFlow.Application.Holidays;

internal static class HolidayValidation
{
    internal static People.ValidationResult ValidateOrganization(OrganizationHolidayInput input)
    {
        return Validate(input.Name, input.StartDate, input.EndDate);
    }

    internal static People.ValidationResult ValidateOfficial(OfficialHolidayInput input)
    {
        return Validate(input.Name, input.StartDate, input.EndDate);
    }

    private static People.ValidationResult Validate(string name, DateOnly? startDate, DateOnly? endDate)
    {
        var result = new People.ValidationResult();

        if (string.IsNullOrWhiteSpace(name))
        {
            result.Add("Name", "This field is required.");
        }
        else if (name.Length > 160)
        {
            result.Add("Name", "This field must be 160 characters or fewer.");
        }

        if (startDate is null)
        {
            result.Add("StartDate", "This field is required.");
        }

        if (endDate is null)
        {
            result.Add("EndDate", "This field is required.");
        }

        if (startDate is not null && endDate is not null && startDate > endDate)
        {
            result.Add("EndDate", "End date must be on or after start date.");
        }

        return result;
    }
}
