namespace LeaveFlow.Application.LeaveRequests;

public static class LeaveConflictDetector
{
    public static bool Overlaps(DateOnly requestStart, DateOnly requestEnd, DateOnly existingStart, DateOnly existingEnd)
    {
        return requestStart <= existingEnd && requestEnd >= existingStart;
    }
}
