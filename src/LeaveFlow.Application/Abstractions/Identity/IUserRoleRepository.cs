namespace LeaveFlow.Application.Abstractions.Identity;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
