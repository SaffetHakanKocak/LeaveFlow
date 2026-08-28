using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Calendar;

internal static class OrganizationCalendarValidation
{
    internal static ValidationResult Validate(OrganizationCalendarQuery query)
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
            else if (OrganizationCalendarDateRange.CountInclusiveDays(query.StartDate.Value, query.EndDate.Value) > OrganizationCalendarSettings.MaxRangeDays)
            {
                result.Add("EndDate", $"Takvim aralığı {OrganizationCalendarSettings.MaxRangeDays} günü aşamaz.");
            }
        }

        return result;
    }
}
