using EasyStock.Tests.Infrastructure;
using EasyStock.Web.Data;
using EasyStock.Web.Models;
using EasyStock.Web.Services;
using EasyStock.Web.ViewModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Tests.Integration;

public class AccountTests(PostgresFixture fixture) : DatabaseTest(fixture)
{
    private static AuthService CreateAuthService(ApplicationDbContext db) => new(db, new PasswordHasher<User>());

    private static UserService CreateUserService(ApplicationDbContext db) => new(db, new PasswordHasher<User>());

    [Fact]
    public async Task ValidateCredentials_AcceptsAnyUsernameCaseAndSurroundingSpaces()
    {
        await TestData.AddUserAsync(Db, "admin");

        await using var context = CreateContext();
        var user = await CreateAuthService(context).ValidateCredentialsAsync("  ADMIN ", TestData.Password);

        Assert.NotNull(user);
        Assert.Equal("admin", user.Username);
    }

    [Fact]
    public async Task ValidateCredentials_RejectsAWrongPassword()
    {
        await TestData.AddUserAsync(Db, "admin");

        await using var context = CreateContext();
        var user = await CreateAuthService(context).ValidateCredentialsAsync("admin", "wrong-password");

        Assert.Null(user);
    }

    [Fact]
    public async Task ValidateCredentials_RejectsAnInactiveUser()
    {
        await TestData.AddUserAsync(Db, "jdoe", UserRole.Operator, isActive: false);

        await using var context = CreateContext();
        var user = await CreateAuthService(context).ValidateCredentialsAsync("jdoe", TestData.Password);

        Assert.Null(user);
    }

    [Fact]
    public async Task CreateUser_NormalizesTheUsernameAndHashesThePassword()
    {
        await using (var context = CreateContext())
        {
            var result = await CreateUserService(context).CreateAsync(new UserCreateViewModel
            {
                Username = "JDoe",
                FullName = "John Doe",
                Role = UserRole.Operator,
                Password = "Secret123!",
                ConfirmPassword = "Secret123!",
            });
            Assert.True(result.Succeeded);
        }

        var user = await Db.Users.AsNoTracking().SingleAsync();
        Assert.Equal("jdoe", user.Username);
        Assert.NotEqual("Secret123!", user.PasswordHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, "Secret123!"));
    }

    [Fact]
    public async Task UpdateUser_AdminCannotDeactivateTheirOwnAccount()
    {
        var admin = await TestData.AddUserAsync(Db, "admin");

        await using (var context = CreateContext())
        {
            var result = await CreateUserService(context).UpdateAsync(new UserEditViewModel
            {
                Id = admin.Id,
                Username = admin.Username,
                FullName = admin.FullName,
                Role = UserRole.Admin,
                IsActive = false,
            }, currentUserId: admin.Id);

            Assert.False(result.Succeeded);
        }

        Assert.True(await Db.Users.AsNoTracking().Where(u => u.Id == admin.Id).Select(u => u.IsActive).SingleAsync());
    }
}