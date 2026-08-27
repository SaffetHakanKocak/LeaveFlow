using LeaveFlow.Application.Abstractions.Authorization;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Domain.Identity;

namespace LeaveFlow.Application.Authorization;

public sealed class ConsultantResourceAuthorizationService(
    IUserRoleRepository userRoleRepository,
    IConsultantIdentityRepository consultantIdentityRepository,
    IManagerIdentityRepository managerIdentityRepository) : IConsultantResourceAuthorizationService
{
    public async Task<bool> CanAccessConsultantAsync(
        Guid actorUserId,
        Guid consultantId,
        CancellationToken cancellationToken = default)
    {
        var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(actorUserId, cancellationToken);

        if (roles.Contains(RoleNames.Administrator, StringComparer.Ordinal))
        {
            return true;
        }

        var ownConsultantId = await consultantIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
        if (ownConsultantId == consultantId)
        {
            return true;
        }

        if (roles.Contains(RoleNames.Manager, StringComparer.Ordinal))
        {
            var managerId = await managerIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
            if (managerId is Guid resolvedManagerId)
            {
                return await managerIdentityRepository.IsAssignedToConsultantAsync(
                    resolvedManagerId,
                    consultantId,
                    cancellationToken);
            }
        }

        return false;
    }
}
