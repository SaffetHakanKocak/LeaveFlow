using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;

namespace LeaveFlow.Application.Abstractions.LeaveRequests;

public interface ILeaveRequestRepository
{
    Task<PagedResult<LeaveRequestListItem>> GetMineAsync(
        Guid consultantId,
        LeaveRequestSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestDetail?> GetByIdAsync(
        Guid leaveRequestId,
        Guid consultantId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsOverlapAsync(
        Guid consultantId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        Guid consultantId,
        LeaveRequestInput input,
        CancellationToken cancellationToken = default);
}
