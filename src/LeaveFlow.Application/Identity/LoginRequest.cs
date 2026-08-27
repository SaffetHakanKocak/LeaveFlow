namespace LeaveFlow.Application.Identity;

public sealed record LoginRequest(
    string Email,
    string Password,
    string? IpAddress,
    string? CorrelationId);
