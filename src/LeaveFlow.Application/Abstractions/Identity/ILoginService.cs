using LeaveFlow.Application.Identity;

namespace LeaveFlow.Application.Abstractions.Identity;

public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task RecordLogoutAsync(Guid userId, string? correlationId, CancellationToken cancellationToken = default);
}
