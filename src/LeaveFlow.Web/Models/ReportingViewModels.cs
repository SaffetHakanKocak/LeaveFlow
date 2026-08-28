using LeaveFlow.Application.Reporting;

namespace LeaveFlow.Web.Models;

public sealed class DashboardViewModel
{
    public required DashboardSummary Dashboard { get; init; }
}

public sealed class ReportsViewModel
{
    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public Guid? ManagerId { get; init; }

    public Guid? ConsultantId { get; init; }

    public ReportsResult? Reports { get; init; }
}
