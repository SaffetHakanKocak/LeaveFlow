using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Calendar;

internal static class OrganizationCalendarValidation
{
    internal static ValidationResult Validate(OrganizationCalendarQuery query)
    {
        var result = new ValidationResult();

        if (query.StartDate is null)
        {
            result.Add("StartDate", "This field is required.");
        }

        if (query.EndDate is null)
        {
            result.Add("EndDate", "This field is required.");
        }

        if (query.StartDate is not null && query.EndDate is not null)
        {
            if (query.StartDate > query.EndDate)
            {
                result.Add("EndDate", "End date must be on or after start date.");
            }
            else if (OrganizationCalendarDateRange.CountInclusiveDays(query.StartDate.Value, query.EndDate.Value) > OrganizationCalendarSettings.MaxRangeDays)
            {
                result.Add("EndDate", $"Calendar range cannot exceed {OrganizationCalendarSettings.MaxRangeDays} days.");
            }
        }

        return result;
    }
}
