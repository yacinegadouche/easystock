using EasyStock.Tests.Infrastructure;
using EasyStock.Web.Models;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Tests.Integration;

public class CategoryServiceTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Create_WithAnExistingNameInAnotherCase_IsRejected()
    {
        Db.Categories.Add(new Category { Name = "Laptops" });
        await Db.SaveChangesAsync();

        await using var context = CreateContext();
        var result = await new CategoryService(context).CreateAsync(new CategoryFormViewModel { Name = "  laptops " });

        Assert.False(result.Succeeded);
        Assert.Equal(1, await Db.Categories.CountAsync());
    }

    [Fact]
    public async Task Delete_ACategoryThatStillContainsItems_IsRejected()
    {
        var item = await TestData.AddItemAsync(Db);

        await using var context = CreateContext();
        var result = await new CategoryService(context).DeleteAsync(item.CategoryId);

        Assert.False(result.Succeeded);
        Assert.True(await Db.Categories.AnyAsync(c => c.Id == item.CategoryId));
    }
}