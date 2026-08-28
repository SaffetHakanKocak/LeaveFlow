using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Reporting;

internal static class ReportingValidation
{
    internal static ValidationResult Validate(ReportQuery query)
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
            else if (query.EndDate.Value.DayNumber - query.StartDate.Value.DayNumber + 1 > ReportingSettings.MaxRangeDays)
            {
                result.Add("EndDate", $"Report range cannot exceed {ReportingSettings.MaxRangeDays} days.");
            }
        }

        return result;
    }
}
