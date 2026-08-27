using LeaveFlow.Application.Identity;

namespace LeaveFlow.Application.Abstractions.Identity;

public interface ILoginAttemptRepository
{
    Task InsertAsync(LoginAttemptRecord record, CancellationToken cancellationToken = default);
}
