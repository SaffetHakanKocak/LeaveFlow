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
            result.Add("Name", "Bu alan zorunludur.");
        }
        else if (name.Length > 160)
        {
            result.Add("Name", "Bu alan en fazla 160 karakter olabilir.");
        }

        if (startDate is null)
        {
            result.Add("StartDate", "Bu alan zorunludur.");
        }

        if (endDate is null)
        {
            result.Add("EndDate", "Bu alan zorunludur.");
        }

        if (startDate is not null && endDate is not null && startDate > endDate)
        {
            result.Add("EndDate", "Bitiş tarihi başlangıç tarihinde veya sonrasında olmalıdır.");
        }

        return result;
    }
}
