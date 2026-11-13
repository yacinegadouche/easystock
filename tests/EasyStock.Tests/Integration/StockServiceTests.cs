using EasyStock.Tests.Infrastructure;
using EasyStock.Web.Data;
using EasyStock.Web.Models;
using EasyStock.Web.Notifications;
using EasyStock.Web.Services;

using Microsoft.EntityFrameworkCore;

namespace EasyStock.Tests.Integration;

public class StockServiceTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    private readonly LowStockAlertQueue alertQueue = new();

    private StockService CreateService(ApplicationDbContext db) => new(db, alertQueue);

    [Fact]
    public async Task Entry_IncreasesStockAndRecordsTheMovement()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 5);

        await using var context = CreateContext();
        var result = await CreateService(context)
            .RecordManualMovementAsync(MovementType.Entry, item.Id, 3, user.Id, "Purchase", "PO-1");

        Assert.True(result.Succeeded);
        Assert.Equal(8, await GetStockAsync(item.Id));

        var movement = await Db.StockMovements.AsNoTracking().SingleAsync();
        Assert.Equal(MovementType.Entry, movement.Type);
        Assert.Equal(3, movement.Quantity);
        Assert.Equal(8, movement.QuantityAfter);
        Assert.Equal("PO-1", movement.ExternalReference);
        Assert.Equal(user.Id, movement.UserId);
    }

    [Fact]
    public async Task Exit_MoreThanStock_FailsAndChangesNothing()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 5);

        await using var context = CreateContext();
        var result = await CreateService(context)
            .RecordManualMovementAsync(MovementType.Exit, item.Id, 6, user.Id, "Lost", null);

        Assert.False(result.Succeeded);
        Assert.Equal("Not enough stock available.", result.Error);
        Assert.Equal(5, await GetStockAsync(item.Id));
        Assert.False(await Db.StockMovements.AnyAsync());
    }

    [Fact]
    public async Task Exit_WithoutReason_IsRejected()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 5);

        await using var context = CreateContext();
        var result = await CreateService(context)
            .RecordManualMovementAsync(MovementType.Exit, item.Id, 1, user.Id, "   ", null);

        Assert.False(result.Succeeded);
        Assert.Equal(5, await GetStockAsync(item.Id));
    }

    [Fact]
    public async Task LowStockAlert_IsRaisedOnlyWhenCrossingTheMinimum()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 5, minimum: 2);

        async Task ExitAsync(int quantity)
        {
            await using var context = CreateContext();
            var result = await CreateService(context)
                .RecordManualMovementAsync(MovementType.Exit, item.Id, quantity, user.Id, "Used", null);
            Assert.True(result.Succeeded);
        }

        await ExitAsync(2);
        Assert.False(alertQueue.TryDequeue(out _));

        await ExitAsync(1);
        Assert.True(alertQueue.TryDequeue(out var alert));
        Assert.Equal(item.Id, alert.ItemId);
        Assert.Equal(2, alert.QuantityInStock);

        await ExitAsync(1);
        Assert.False(alertQueue.TryDequeue(out _));
    }

    [Fact]
    public async Task RecordMovement_OutsideATransaction_Throws()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db);

        await using var context = CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(context).RecordMovementAsync(item.Id, MovementType.Entry, 1, user.Id));
    }
}