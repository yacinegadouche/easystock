using EasyStock.Web.Data;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Testcontainers.PostgreSql;

namespace EasyStock.Tests.Infrastructure;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:17")
        .Build();

    public Task InitializeAsync() => container.StartAsync();

    public Task DisposeAsync() => container.DisposeAsync().AsTask();

    public string CreateDatabaseConnectionString() =>
        new NpgsqlConnectionStringBuilder(container.GetConnectionString())
        {
            Database = $"test_{Guid.NewGuid():N}",
        }.ConnectionString;

    public static ApplicationDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options);
}

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Database";
}