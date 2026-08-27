using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.People;

public interface IManagerConsultantAssignmentRepository
{
    Task<IReadOnlyList<ManagerConsultantAssignment>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);

    Task<bool> AssignAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(Guid managerId, Guid consultantId, CancellationToken cancellationToken = default);
}
