namespace LeaveFlow.Application.Identity;

public sealed record AuditLogRecord(
    Guid? ActorUserId,
    string Action,
    string? TargetType,
    string? TargetId,
    string Outcome,
    string? CorrelationId,
    string? MetadataJson);
