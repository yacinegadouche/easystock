using EasyStock.Web.Data;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Tests.Infrastructure;

[Collection(DatabaseCollection.Name)]
public abstract class DatabaseTest(PostgresFixture fixture) : IAsyncLifetime
{
    private string connectionString = string.Empty;

    protected ApplicationDbContext Db { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        connectionString = fixture.CreateDatabaseConnectionString();
        Db = CreateContext();
        await Db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Db.Database.EnsureDeletedAsync();
        await Db.DisposeAsync();
    }

    protected ApplicationDbContext CreateContext() => PostgresFixture.CreateContext(connectionString);

    protected Task<int> GetStockAsync(int itemId) =>
        Db.Items.AsNoTracking()
            .Where(i => i.Id == itemId)
            .Select(i => i.QuantityInStock)
            .SingleAsync();
}