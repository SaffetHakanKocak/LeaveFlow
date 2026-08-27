using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.LeaveRequests;

public interface ILeaveRequestService
{
    Task<PagedResult<LeaveRequestListItem>?> GetMineAsync(
        Guid actorUserId,
        LeaveRequestSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestDetail?> GetMineByIdAsync(
        Guid actorUserId,
        Guid leaveRequestId,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestCreateResult> CreateMineAsync(
        Guid actorUserId,
        LeaveRequestInput input,
        CancellationToken cancellationToken = default);

    ValidationResult Validate(LeaveRequestInput input);
}
