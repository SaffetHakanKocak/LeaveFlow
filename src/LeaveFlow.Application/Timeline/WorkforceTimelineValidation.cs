using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Timeline;

internal static class WorkforceTimelineValidation
{
    internal static ValidationResult Validate(WorkforceTimelineQuery query)
    {
        var result = new ValidationResult();

        if (query.StartDate is null)
        {
            result.Add("StartDate", "Bu alan zorunludur.");
        }

        if (query.EndDate is null)
        {
            result.Add("EndDate", "Bu alan zorunludur.");
        }

        if (query.StartDate is not null && query.EndDate is not null)
        {
            if (query.StartDate > query.EndDate)
            {
                result.Add("EndDate", "Bitiş tarihi başlangıç tarihinde veya sonrasında olmalıdır.");
            }
            else if (TimelineDateRange.CountInclusiveDays(query.StartDate.Value, query.EndDate.Value) > WorkforceTimelineSettings.MaxRangeDays)
            {
                result.Add("EndDate", $"Zaman çizelgesi aralığı {WorkforceTimelineSettings.MaxRangeDays} günü aşamaz.");
            }
        }

        return result;
    }
}
