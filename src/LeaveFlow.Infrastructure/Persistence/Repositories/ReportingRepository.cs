using System.Data;
using Dapper;
using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Reporting;
using LeaveFlow.Infrastructure.Persistence.SqlServer;

namespace LeaveFlow.Infrastructure.Persistence.Repositories;

public sealed class ReportingRepository(IDbConnectionFactory connectionFactory) : IReportingRepository
{
    public Task<DashboardData> GetDashboardForAdminAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Today", today, DbType.Date);
        return QuerySingleAsync<DashboardData>(StoredProcedureNames.DashboardGetForAdmin, parameters, cancellationToken);
    }

    public Task<DashboardData> GetDashboardForManagerAsync(Guid managerId, DateOnly today, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("ManagerId", managerId, DbType.Guid);
        parameters.Add("Today", today, DbType.Date);
        return QuerySingleAsync<DashboardData>(StoredProcedureNames.DashboardGetForManager, parameters, cancellationToken);
    }

    public Task<DashboardData> GetDashboardForConsultantAsync(Guid consultantId, DateOnly today, CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("ConsultantId", consultantId, DbType.Guid);
        parameters.Add("Today", today, DbType.Date);
        return QuerySingleAsync<DashboardData>(StoredProcedureNames.DashboardGetForConsultant, parameters, cancellationToken);
    }

    public Task<IReadOnlyList<LeaveUsageReportRow>> GetConsultantLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<LeaveUsageReportRow>(StoredProcedureNames.ReportsConsultantLeaveUsage, CreateReportParameters(query), cancellationToken);
    }

    public Task<IReadOnlyList<MonthlyLeaveActivityRow>> GetMonthlyLeaveActivityAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<MonthlyLeaveActivityRow>(StoredProcedureNames.ReportsMonthlyLeaveActivity, CreateReportParameters(query), cancellationToken);
    }

    public Task<IReadOnlyList<TeamLeaveUsageRow>> GetTeamLeaveUsageAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<TeamLeaveUsageRow>(StoredProcedureNames.ReportsTeamLeaveUsage, CreateReportParameters(query), cancellationToken);
    }

    public Task<IReadOnlyList<PeakLeaveDay>> GetPeakLeaveDaysAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<PeakLeaveDay>(StoredProcedureNames.ReportsPeakLeaveDays, CreateReportParameters(query), cancellationToken);
    }

    public Task<IReadOnlyList<StatusDistributionItem>> GetStatusDistributionAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<StatusDistributionItem>(StoredProcedureNames.ReportsStatusDistribution, CreateReportParameters(query), cancellationToken);
    }

    public Task<IReadOnlyList<UpcomingLeaveItem>> GetUpcomingLeavesAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<UpcomingLeaveItem>(StoredProcedureNames.ReportsUpcomingLeaves, CreateReportParameters(query), cancellationToken);
    }

    public Task<IReadOnlyList<UpcomingHolidayItem>> GetUpcomingHolidaysAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<UpcomingHolidayItem>(StoredProcedureNames.ReportsUpcomingHolidays, CreateReportParameters(query), cancellationToken);
    }

    public Task<IReadOnlyList<LeaveRequestSnapshot>> GetRecentLeaveRequestsAsync(ReportQuery query, CancellationToken cancellationToken = default)
    {
        return QueryListAsync<LeaveRequestSnapshot>(StoredProcedureNames.ReportsRecentLeaveRequests, CreateReportParameters(query), cancellationToken);
    }

    private async Task<T> QuerySingleAsync<T>(string storedProcedureName, DynamicParameters parameters, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<T>(new CommandDefinition(
            storedProcedureName,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));
    }

    private async Task<IReadOnlyList<T>> QueryListAsync<T>(string storedProcedureName, DynamicParameters parameters, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return (await connection.QueryAsync<T>(new CommandDefinition(
            storedProcedureName,
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken))).AsList();
    }

    private static DynamicParameters CreateReportParameters(ReportQuery query)
    {
        var parameters = new DynamicParameters();
        parameters.Add("StartDate", query.StartDate, DbType.Date);
        parameters.Add("EndDate", query.EndDate, DbType.Date);
        parameters.Add("ManagerId", query.ManagerId, DbType.Guid);
        parameters.Add("ConsultantId", query.ConsultantId, DbType.Guid);
        return parameters;
    }
}
