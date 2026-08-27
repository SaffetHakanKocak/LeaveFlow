using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.People;
using LeaveFlow.Domain.LeaveRequests;

namespace LeaveFlow.Application.LeaveRequests;

public sealed class LeaveRequestService(
    ILeaveRequestRepository repository,
    IConsultantIdentityRepository consultantIdentityRepository,
    IConsultantManagementRepository consultantManagementRepository,
    TimeProvider timeProvider) : ILeaveRequestService
{
    public async Task<PagedResult<LeaveRequestListItem>?> GetMineAsync(
        Guid actorUserId,
        LeaveRequestSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var consultant = await ResolveActiveConsultantAsync(actorUserId, cancellationToken);
        if (consultant is null)
        {
            return null;
        }

        return await repository.GetMineAsync(consultant.Id, Normalize(request), cancellationToken);
    }

    public async Task<LeaveRequestDetail?> GetMineByIdAsync(
        Guid actorUserId,
        Guid leaveRequestId,
        CancellationToken cancellationToken = default)
    {
        var consultant = await ResolveActiveConsultantAsync(actorUserId, cancellationToken);
        return consultant is null
            ? null
            : await repository.GetByIdAsync(leaveRequestId, consultant.Id, cancellationToken);
    }

    public async Task<LeaveRequestCreateResult> CreateMineAsync(
        Guid actorUserId,
        LeaveRequestInput input,
        CancellationToken cancellationToken = default)
    {
        if (!Validate(input).IsValid)
        {
            return LeaveRequestCreateResult.Failure("ValidationFailed");
        }

        var consultant = await ResolveActiveConsultantAsync(actorUserId, cancellationToken);
        if (consultant is null)
        {
            return LeaveRequestCreateResult.Failure("InvalidConsultant");
        }

        var startDate = input.StartDate.GetValueOrDefault();
        var endDate = input.EndDate.GetValueOrDefault();
        if (await repository.ExistsOverlapAsync(consultant.Id, startDate, endDate, cancellationToken))
        {
            return LeaveRequestCreateResult.Failure("Overlap");
        }

        var leaveRequestId = await repository.CreateAsync(consultant.Id, input, cancellationToken);
        return LeaveRequestCreateResult.Success(leaveRequestId);
    }

    public ValidationResult Validate(LeaveRequestInput input)
    {
        return LeaveRequestValidation.Validate(input, DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
    }

    private async Task<ConsultantDetail?> ResolveActiveConsultantAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var consultantId = await consultantIdentityRepository.GetIdByUserIdAsync(actorUserId, cancellationToken);
        if (consultantId is null)
        {
            return null;
        }

        var consultant = await consultantManagementRepository.GetByIdAsync(consultantId.Value, cancellationToken);
        return consultant?.IsActive == true ? consultant : null;
    }

    private static LeaveRequestSearchRequest Normalize(LeaveRequestSearchRequest request)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();
        if (status is not null && !LeaveRequestStatuses.All.Contains(status))
        {
            status = null;
        }

        return request with
        {
            Status = status,
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
    }
}
