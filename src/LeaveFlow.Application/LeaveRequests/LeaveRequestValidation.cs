using LeaveFlow.Application.People;

namespace LeaveFlow.Application.LeaveRequests;

internal static class LeaveRequestValidation
{
    internal static ValidationResult Validate(LeaveRequestInput input, DateOnly today)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            result.Add("Reason", "This field is required.");
        }
        else if (input.Reason.Length > 512)
        {
            result.Add("Reason", "This field must be 512 characters or fewer.");
        }

        if (input.StartDate is null)
        {
            result.Add("StartDate", "This field is required.");
        }

        if (input.EndDate is null)
        {
            result.Add("EndDate", "This field is required.");
        }

        if (input.StartDate is not null && input.EndDate is not null)
        {
            if (input.StartDate > input.EndDate)
            {
                result.Add("EndDate", "End date must be on or after start date.");
            }

            if (input.StartDate < today)
            {
                result.Add("StartDate", "Start date cannot be in the past.");
            }
        }

        return result;
    }
}
