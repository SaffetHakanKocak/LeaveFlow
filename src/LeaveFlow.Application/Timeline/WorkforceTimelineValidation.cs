using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Timeline;

internal static class WorkforceTimelineValidation
{
    internal static ValidationResult Validate(WorkforceTimelineQuery query)
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
            else if (TimelineDateRange.CountInclusiveDays(query.StartDate.Value, query.EndDate.Value) > WorkforceTimelineSettings.MaxRangeDays)
            {
                result.Add("EndDate", $"Timeline range cannot exceed {WorkforceTimelineSettings.MaxRangeDays} days.");
            }
        }

        return result;
    }
}
