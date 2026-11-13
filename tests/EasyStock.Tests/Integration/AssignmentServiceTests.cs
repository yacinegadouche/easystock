using EasyStock.Tests.Infrastructure;
using EasyStock.Web.Data;
using EasyStock.Web.Models;
using EasyStock.Web.Notifications;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Tests.Integration;

public class AssignmentServiceTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    private static AssignmentService CreateService(ApplicationDbContext db) =>
        new(db, new StockService(db, new LowStockAlertQueue()));

    private static AssignmentCreateViewModel Request(int employeeId, int itemId, int quantity) =>
        new() { EmployeeId = employeeId, ItemId = itemId, Quantity = quantity };

    [Fact]
    public async Task Create_DecreasesStockAndLinksTheMovementToTheAssignment()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 10);
        var employee = await TestData.AddEmployeeAsync(Db);

        await using var context = CreateContext();
        var result = await CreateService(context).CreateAsync(Request(employee.Id, item.Id, 2), user.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(8, await GetStockAsync(item.Id));

        var assignment = await Db.Assignments.AsNoTracking().SingleAsync();
        var movement = await Db.StockMovements.AsNoTracking().SingleAsync();
        Assert.Equal(MovementType.Assignment, movement.Type);
        Assert.Equal(assignment.Id, movement.AssignmentId);
    }

    [Fact]
    public async Task Create_WithMoreThanStock_RollsBackTheAssignment()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 1);
        var employee = await TestData.AddEmployeeAsync(Db);

        await using var context = CreateContext();
        var result = await CreateService(context).CreateAsync(Request(employee.Id, item.Id, 2), user.Id);

        Assert.False(result.Succeeded);
        Assert.Equal(1, await GetStockAsync(item.Id));
        Assert.False(await Db.Assignments.AnyAsync());
        Assert.False(await Db.StockMovements.AnyAsync());
    }

    [Fact]
    public async Task Create_ForAnInactiveEmployee_IsRejected()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db);
        var employee = await TestData.AddEmployeeAsync(Db, isActive: false);

        await using var context = CreateContext();
        var result = await CreateService(context).CreateAsync(Request(employee.Id, item.Id, 1), user.Id);

        Assert.False(result.Succeeded);
        Assert.Equal(10, await GetStockAsync(item.Id));
    }

    [Fact]
    public async Task Return_RestoresStockAndCannotBeDoneTwice()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 10);
        var employee = await TestData.AddEmployeeAsync(Db);

        await using (var context = CreateContext())
        {
            await CreateService(context).CreateAsync(Request(employee.Id, item.Id, 3), user.Id);
        }

        var assignmentId = await Db.Assignments.Select(a => a.Id).SingleAsync();

        await using (var context = CreateContext())
        {
            var firstReturn = await CreateService(context).ReturnAsync(assignmentId, user.Id);
            Assert.True(firstReturn.Succeeded);
        }

        await using (var context = CreateContext())
        {
            var secondReturn = await CreateService(context).ReturnAsync(assignmentId, user.Id);
            Assert.False(secondReturn.Succeeded);
        }

        Assert.Equal(10, await GetStockAsync(item.Id));
        Assert.Equal(2, await Db.StockMovements.CountAsync());
    }

    [Fact]
    public async Task ConcurrentAssignmentsOfTheLastUnit_OnlyOneSucceeds()
    {
        var user = await TestData.AddUserAsync(Db);
        var item = await TestData.AddItemAsync(Db, quantity: 1);
        var employee = await TestData.AddEmployeeAsync(Db);

        var attempts = Enumerable.Range(0, 2).Select(async _ =>
        {
            await using var context = CreateContext();
            return await CreateService(context).CreateAsync(Request(employee.Id, item.Id, 1), user.Id);
        });

        var results = await Task.WhenAll(attempts);

        Assert.Single(results, r => r.Succeeded);
        Assert.Equal(0, await GetStockAsync(item.Id));
        Assert.Equal(1, await Db.Assignments.CountAsync());
    }
}