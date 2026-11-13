using EasyStock.Web.Data;
using EasyStock.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace EasyStock.Tests.Infrastructure;

public static class TestData
{
    public const string Password = "Password123!";

    public static async Task<User> AddUserAsync(
        ApplicationDbContext db,
        string username = "admin",
        UserRole role = UserRole.Admin,
        bool isActive = true)
    {
        var user = new User
        {
            Username = username,
            FullName = $"Test {username}",
            Role = role,
            IsActive = isActive,
            PasswordHash = "",
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<Item> AddItemAsync(
        ApplicationDbContext db,
        string name = "Laptop",
        int quantity = 10,
        int minimum = 2)
    {
        var item = new Item
        {
            Name = name,
            Category = new Category { Name = $"{name} category" },
            QuantityInStock = quantity,
            MinimumQuantity = minimum,
        };

        db.Items.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public static async Task<Employee> AddEmployeeAsync(
        ApplicationDbContext db,
        string firstName = "Jane",
        bool isActive = true)
    {
        var employee = new Employee
        {
            FirstName = firstName,
            LastName = "Doe",
            Email = $"{firstName.ToLowerInvariant()}.doe@example.com",
            IsActive = isActive,
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return employee;
    }
}