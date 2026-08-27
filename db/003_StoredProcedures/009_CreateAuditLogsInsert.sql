CREATE OR ALTER PROCEDURE dbo.usp_AuditLogs_Insert
    @ActorUserId uniqueidentifier = NULL,
    @Action nvarchar(128),
    @TargetType nvarchar(128) = NULL,
    @TargetId nvarchar(128) = NULL,
    @Outcome nvarchar(32),
    @CorrelationId nvarchar(128) = NULL,
    @MetadataJson nvarchar(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AuditLogs
    (
        ActorUserId,
        Action,
        TargetType,
        TargetId,
        Outcome,
        CorrelationId,
        MetadataJson
    )
    VALUES
    (
        @ActorUserId,
        @Action,
        @TargetType,
        @TargetId,
        @Outcome,
        @CorrelationId,
        @MetadataJson
    );
END;
