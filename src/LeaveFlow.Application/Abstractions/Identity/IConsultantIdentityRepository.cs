namespace LeaveFlow.Application.Abstractions.Identity;

public interface IConsultantIdentityRepository
{
    Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
