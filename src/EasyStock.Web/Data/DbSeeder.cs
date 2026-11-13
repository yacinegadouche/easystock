using EasyStock.Web.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        string? demoPassword,
        CancellationToken cancellationToken = default)
    {
        await SeedReferenceDataAsync(db, cancellationToken);

        if (!string.IsNullOrWhiteSpace(demoPassword))
        {
            await SeedUsersAsync(db, demoPassword, cancellationToken);
            await SeedInitialStockMovementsAsync(db, cancellationToken);
        }
    }

    private static async Task SeedUsersAsync(
        ApplicationDbContext db,
        string demoPassword,
        CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var hasher = new PasswordHasher<User>();

        User[] users =
        [
            new() { Username = "admin", FullName = "Alex Martin", Role = UserRole.Admin, PasswordHash = "" },
            new() { Username = "manager", FullName = "Nadia Pelletier", Role = UserRole.Manager, PasswordHash = "" },
            new() { Username = "operator", FullName = "Samuel Fortin", Role = UserRole.Operator, PasswordHash = "" },
        ];

        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, demoPassword);
        }

        db.Users.AddRange(users);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedReferenceDataAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var laptops = new Category { Name = "Laptops", Description = "Portable computers" };
        var desktops = new Category { Name = "Desktops", Description = "Fixed workstations" };
        var networking = new Category { Name = "Networking", Description = "Switches, routers and access points" };
        var storage = new Category { Name = "Storage", Description = "NAS, hard drives and SSDs" };
        var peripherals = new Category { Name = "Peripherals", Description = "Monitors, keyboards and mice" };
        var security = new Category { Name = "Security", Description = "Firewalls and security appliances" };

        var dell = new Supplier { Name = "Dell Technologies", ContactEmail = "sales@dell.example", Phone = "800-555-0101" };
        var lenovo = new Supplier { Name = "Lenovo Canada", ContactEmail = "pro@lenovo.example", Phone = "866-555-0102" };
        var cdw = new Supplier { Name = "CDW Canada", ContactEmail = "orders@cdw.example", Phone = "416-555-0103" };
        var cisco = new Supplier { Name = "Cisco Partner", ContactEmail = "network@cisco.example", Phone = "450-555-0104" };
        var softchoice = new Supplier { Name = "Softchoice", ContactEmail = "supply@softchoice.example", Phone = "416-555-0105" };

        db.Categories.AddRange(laptops, desktops, networking, storage, peripherals, security);
        db.Suppliers.AddRange(dell, lenovo, cdw, cisco, softchoice);

        db.Items.AddRange(
            new Item { Name = "Dell Latitude 5540", Description = "i7, 16 GB, 512 GB SSD", Category = laptops, Supplier = dell, QuantityInStock = 14, MinimumQuantity = 5 },
            new Item { Name = "Lenovo ThinkPad T14 Gen 4", Description = "Standard operations laptop", Category = laptops, Supplier = lenovo, QuantityInStock = 11, MinimumQuantity = 4 },
            new Item { Name = "Dell OptiPlex 7010 SFF", Description = "Front desk workstation", Category = desktops, Supplier = dell, QuantityInStock = 6, MinimumQuantity = 2 },
            new Item { Name = "Cisco Catalyst C9200L-24P", Description = "24-port PoE switch", Category = networking, Supplier = cisco, QuantityInStock = 3, MinimumQuantity = 1 },
            new Item { Name = "Ubiquiti UniFi U6-LR", Description = "Office Wi-Fi access point", Category = networking, Supplier = cdw, QuantityInStock = 12, MinimumQuantity = 4 },
            new Item { Name = "Synology DS923+", Description = "Department backup NAS", Category = storage, Supplier = cdw, QuantityInStock = 4, MinimumQuantity = 2 },
            new Item { Name = "Samsung 990 PRO 1 TB", Description = "Spare laptop NVMe SSD", Category = storage, Supplier = cdw, QuantityInStock = 20, MinimumQuantity = 8 },
            new Item { Name = "Dell P2723DE 27\" Monitor", Description = "QHD USB-C monitor", Category = peripherals, Supplier = dell, QuantityInStock = 3, MinimumQuantity = 6 },
            new Item { Name = "Logitech MX Keys", Description = "Wireless keyboard", Category = peripherals, Supplier = softchoice, QuantityInStock = 25, MinimumQuantity = 10 },
            new Item { Name = "FortiGate 60F", Description = "Main site firewall", Category = security, Supplier = softchoice, QuantityInStock = 1, MinimumQuantity = 1 });

        db.Employees.AddRange(
            new Employee { FirstName = "Sophie", LastName = "Tremblay", Email = "sophie.tremblay@easystock.example", Department = "Finance" },
            new Employee { FirstName = "Marc", LastName = "Gagnon", Email = "marc.gagnon@easystock.example", Department = "Operations" },
            new Employee { FirstName = "Amina", LastName = "Benali", Email = "amina.benali@easystock.example", Department = "IT" },
            new Employee { FirstName = "Julien", LastName = "Roy", Email = "julien.roy@easystock.example", Department = "Sales" },
            new Employee { FirstName = "Chloé", LastName = "Côté", Email = "chloe.cote@easystock.example", Department = "Human Resources" },
            new Employee { FirstName = "Karim", LastName = "Haddad", Email = "karim.haddad@easystock.example", Department = "IT" },
            new Employee { FirstName = "Émilie", LastName = "Bouchard", Email = "emilie.bouchard@easystock.example", Department = "Marketing" },
            new Employee { FirstName = "Thomas", LastName = "Lavoie", Email = "thomas.lavoie@easystock.example", Department = "Operations" });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedInitialStockMovementsAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        if (await db.StockMovements.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = await db.Users.SingleOrDefaultAsync(u => u.Username == "admin", cancellationToken);

        if (admin is null)
        {
            return;
        }

        var openingDate = DateTime.UtcNow.AddDays(-30);
        var items = await db.Items.Where(i => i.QuantityInStock > 0).ToListAsync(cancellationToken);

        db.StockMovements.AddRange(items.Select(item => new StockMovement
        {
            ItemId = item.Id,
            Type = MovementType.Entry,
            Quantity = item.QuantityInStock,
            QuantityAfter = item.QuantityInStock,
            Reason = "Initial stock",
            OccurredAt = openingDate,
            UserId = admin.Id,
        }));

        await db.SaveChangesAsync(cancellationToken);
    }
}