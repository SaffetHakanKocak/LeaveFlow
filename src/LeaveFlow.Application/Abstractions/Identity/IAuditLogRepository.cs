using LeaveFlow.Application.Identity;

namespace LeaveFlow.Application.Abstractions.Identity;

public interface IAuditLogRepository
{
    Task InsertAsync(AuditLogRecord record, CancellationToken cancellationToken = default);
}
