using LeaveFlow.Application.People;
using LeaveFlow.Application.Reporting;

namespace LeaveFlow.Application.Abstractions.Reporting;

public interface IReportingService
{
    Task<DashboardSummary?> GetDashboardAsync(Guid actorUserId, CancellationToken cancellationToken = default);

    Task<ReportsResult?> GetReportsAsync(Guid actorUserId, ReportQuery query, CancellationToken cancellationToken = default);

    ValidationResult Validate(ReportQuery query);
}
