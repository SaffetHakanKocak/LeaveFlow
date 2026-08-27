using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Application.People;

namespace LeaveFlow.UnitTests.LeaveRequests;

public sealed class LeaveRequestServiceTests
{
    private static readonly DateTimeOffset Today = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Validate_Should_RequireReasonAndDates()
    {
        var service = CreateService();

        var result = service.Validate(new LeaveRequestInput(string.Empty, null, null));

        Assert.False(result.IsValid);
        Assert.Contains("Reason", result.Errors.Keys);
        Assert.Contains("StartDate", result.Errors.Keys);
        Assert.Contains("EndDate", result.Errors.Keys);
    }

    [Fact]
    public void Validate_Should_RejectInvalidRange()
    {
        var service = CreateService();

        var result = service.Validate(new LeaveRequestInput("Vacation", new DateOnly(2026, 9, 2), new DateOnly(2026, 9, 1)));

        Assert.False(result.IsValid);
        Assert.Contains("EndDate", result.Errors.Keys);
    }

    [Fact]
    public void Validate_Should_RejectPastStartDate()
    {
        var service = CreateService();

        var result = service.Validate(new LeaveRequestInput("Vacation", new DateOnly(2026, 8, 26), new DateOnly(2026, 8, 27)));

        Assert.False(result.IsValid);
        Assert.Contains("StartDate", result.Errors.Keys);
    }

    [Fact]
    public async Task CreateMineAsync_Should_RejectOverlap()
    {
        var repository = new StubLeaveRequestRepository { ExistsOverlap = true };
        var service = CreateService(repository);

        var result = await service.CreateMineAsync(Guid.Parse("10000000-0000-0000-0000-000000000001"), new LeaveRequestInput("Vacation", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)));

        Assert.False(result.Succeeded);
        Assert.Equal("Overlap", result.ErrorCode);
        Assert.False(repository.Created);
    }

    [Fact]
    public async Task CreateMineAsync_Should_RejectInactiveConsultant()
    {
        var service = CreateService(consultantIsActive: false);

        var result = await service.CreateMineAsync(Guid.Parse("10000000-0000-0000-0000-000000000001"), new LeaveRequestInput("Vacation", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 3)));

        Assert.False(result.Succeeded);
        Assert.Equal("InvalidConsultant", result.ErrorCode);
    }

    private static LeaveRequestService CreateService(
        StubLeaveRequestRepository? repository = null,
        bool consultantIsActive = true)
    {
        return new LeaveRequestService(
            repository ?? new StubLeaveRequestRepository(),
            new StubConsultantIdentityRepository(),
            new StubConsultantManagementRepository(consultantIsActive),
            new FixedTimeProvider(Today));
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal sealed class StubConsultantIdentityRepository : IConsultantIdentityRepository
{
    public Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Guid?>(Guid.Parse("20000000-0000-0000-0000-000000000001"));
    }
}

internal sealed class StubConsultantManagementRepository(bool consultantIsActive) : IConsultantManagementRepository
{
    public Task<LeaveFlow.Application.Common.PagedResult<ConsultantListItem>> SearchAsync(PeopleSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LeaveFlow.Application.Common.PagedResult<ConsultantListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<ConsultantDetail?> GetByIdAsync(Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ConsultantDetail?>(new ConsultantDetail(consultantId, Guid.NewGuid(), "Test", "Consultant", "test@leaveflow.test", null, null, null, consultantIsActive));
    }

    public Task<Guid> CreateAsync(ConsultantInput input, CancellationToken cancellationToken = default) => Task.FromResult(Guid.NewGuid());

    public Task<bool> UpdateAsync(Guid consultantId, ConsultantInput input, CancellationToken cancellationToken = default) => Task.FromResult(false);

    public Task<bool> SetActiveAsync(Guid consultantId, bool isActive, CancellationToken cancellationToken = default) => Task.FromResult(false);
}

internal sealed class StubLeaveRequestRepository : ILeaveRequestRepository
{
    public bool ExistsOverlap { get; init; }

    public bool Created { get; private set; }

    public Task<LeaveFlow.Application.Common.PagedResult<LeaveRequestListItem>> GetMineAsync(Guid consultantId, LeaveRequestSearchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LeaveFlow.Application.Common.PagedResult<LeaveRequestListItem>([], request.PageNumber, request.PageSize, 0));
    }

    public Task<LeaveRequestDetail?> GetByIdAsync(Guid leaveRequestId, Guid consultantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<LeaveRequestDetail?>(null);
    }

    public Task<bool> ExistsOverlapAsync(Guid consultantId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ExistsOverlap);
    }

    public Task<Guid> CreateAsync(Guid consultantId, LeaveRequestInput input, CancellationToken cancellationToken = default)
    {
        Created = true;
        return Task.FromResult(Guid.NewGuid());
    }
}
