namespace LeaveFlow.Domain.LeaveRequests;

public static class LeaveRequestStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Pending,
        Approved,
        Rejected
    };
}
