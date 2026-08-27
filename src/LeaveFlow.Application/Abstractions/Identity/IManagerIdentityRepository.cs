namespace LeaveFlow.Application.Abstractions.Identity;

public interface IManagerIdentityRepository
{
    Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IsAssignedToConsultantAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default);
}
