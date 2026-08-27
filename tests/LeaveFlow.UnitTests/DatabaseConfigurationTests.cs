using LeaveFlow.Application.Abstractions.Data;
using LeaveFlow.Infrastructure;
using LeaveFlow.Infrastructure.Persistence.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveFlow.UnitTests;

public sealed class DatabaseConfigurationTests
{
    [Fact]
    public void AddInfrastructure_Should_Register_DefaultDatabaseOptions()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DatabaseOptions>();

        Assert.Equal("DefaultConnection", options.ConnectionStringName);
    }

    [Fact]
    public async Task ConnectionFactory_Should_FailClearly_WhenConnectionStringIsMissing()
    {
        var factory = new SqlServerConnectionFactory(string.Empty);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await factory.CreateOpenConnectionAsync());

        Assert.Contains("connection string is not configured", exception.Message);
    }

    [Fact]
    public void AddInfrastructure_Should_Register_DataAccessServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LeaveFlow:Database:ConnectionStringName"] = "DefaultConnection",
                ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=LeaveFlow;Integrated Security=true;TrustServerCertificate=true"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IDbConnectionFactory>());
        Assert.NotNull(provider.GetRequiredService<IDataTransactionFactory>());
    }
}
