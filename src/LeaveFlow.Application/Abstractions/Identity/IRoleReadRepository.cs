using LeaveFlow.Application.Identity;

namespace LeaveFlow.Application.Abstractions.Identity;

public interface IRoleReadRepository
{
    Task<IReadOnlyList<RoleSummary>> GetAllAsync(CancellationToken cancellationToken = default);
}
