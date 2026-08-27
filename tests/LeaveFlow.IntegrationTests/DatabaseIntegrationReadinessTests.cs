namespace LeaveFlow.IntegrationTests;

public sealed class DatabaseIntegrationReadinessTests
{
    [Fact]
    public void DatabaseIntegrationTests_Should_BeConditionalUntilSqlServerIsConfigured()
    {
        var connectionString = Environment.GetEnvironmentVariable("LEAVEFLOW_TEST_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        Assert.Contains("Database=", connectionString, StringComparison.OrdinalIgnoreCase);
    }
}
