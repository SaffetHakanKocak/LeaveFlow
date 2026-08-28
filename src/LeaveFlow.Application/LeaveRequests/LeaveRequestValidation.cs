using LeaveFlow.Application.People;

namespace LeaveFlow.Application.LeaveRequests;

internal static class LeaveRequestValidation
{
    internal static ValidationResult Validate(LeaveRequestInput input, DateOnly today)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            result.Add("Reason", "Bu alan zorunludur.");
        }
        else if (input.Reason.Length > 512)
        {
            result.Add("Reason", "Bu alan en fazla 512 karakter olabilir.");
        }

        if (input.StartDate is null)
        {
            result.Add("StartDate", "Bu alan zorunludur.");
        }

        if (input.EndDate is null)
        {
            result.Add("EndDate", "Bu alan zorunludur.");
        }

        if (input.StartDate is not null && input.EndDate is not null)
        {
            if (input.StartDate > input.EndDate)
            {
                result.Add("EndDate", "Bitiş tarihi başlangıç tarihinde veya sonrasında olmalıdır.");
            }

            if (input.StartDate < today)
            {
                result.Add("StartDate", "Başlangıç tarihi geçmişte olamaz.");
            }
        }

        return result;
    }
}
