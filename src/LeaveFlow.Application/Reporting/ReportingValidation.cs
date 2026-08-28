using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Reporting;

internal static class ReportingValidation
{
    internal static ValidationResult Validate(ReportQuery query)
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
            else if (query.EndDate.Value.DayNumber - query.StartDate.Value.DayNumber + 1 > ReportingSettings.MaxRangeDays)
            {
                result.Add("EndDate", $"Rapor aralığı {ReportingSettings.MaxRangeDays} günü aşamaz.");
            }
        }

        return result;
    }
}
