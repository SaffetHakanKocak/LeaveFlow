namespace LeaveFlow.Infrastructure.Persistence.SqlServer;

public sealed class DatabaseOptions
{
    public string ConnectionStringName { get; init; } = "DefaultConnection";
}
