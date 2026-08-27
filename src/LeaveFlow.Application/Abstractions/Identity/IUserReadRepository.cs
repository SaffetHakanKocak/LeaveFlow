using LeaveFlow.Application.Identity;

namespace LeaveFlow.Application.Abstractions.Identity;

public interface IUserReadRepository
{
    Task<UserSummary?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
