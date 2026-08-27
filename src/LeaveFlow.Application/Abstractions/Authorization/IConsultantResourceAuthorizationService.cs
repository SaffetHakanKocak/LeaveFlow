namespace LeaveFlow.Application.Abstractions.Authorization;

public interface IConsultantResourceAuthorizationService
{
    Task<bool> CanAccessConsultantAsync(Guid actorUserId, Guid consultantId, CancellationToken cancellationToken = default);
}
