using LeaveFlow.Application.People;

namespace LeaveFlow.Application.LeaveRequests;

internal static class LeaveReviewValidation
{
    internal static ValidationResult Validate(ReviewDecisionInput input)
    {
        var result = new ValidationResult();

        if (input.ReviewNote?.Length > 512)
        {
            result.Add("ReviewNote", "Review note must be 512 characters or fewer.");
        }

        return result;
    }
}
